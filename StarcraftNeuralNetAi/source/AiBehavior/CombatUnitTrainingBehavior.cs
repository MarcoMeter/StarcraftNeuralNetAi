using System;
using System.IO;
using System.Drawing;
using BroodWar.Api;
using BroodWar.Api.Enum;
using Encog.Neural.Networks;
using Encog.Persist;
using NeuralNetTraining.Utility;
using Encog.ML.Data.Basic;
using Encog.ML.Data;

namespace NeuralNetTraining
{
    /// <summary>
    /// Each controlled combat unit should be paired to a CombatUnitTrainingBehavior. This behavior takes care of the learning procedure of the individual neural networks.
    /// </summary>
    public class CombatUnitTrainingBehavior
    {
        #region Member
        private Unit unit;
        private int initialHitPoints;
        private SquadSupervisor squadSupervisor;
        private NeuralNetController neuralNetController;
        private BasicNetwork neuralNet;
        private FitnessMeasure fitnessMeasure;
        private bool requestDecision = true;
        private CombatUnitState currentState = CombatUnitState.SquadState;
        private bool stateTransition = true;
        private Unit currentTarget;
        private int attackAnimationTime = 4;
        private int stateFrameCount = 0;
        private bool trainingMode;
        #endregion

        #region Constructor
        /// <summary>
        /// Create an instance of the AiCombatBehavior.
        /// </summary>
        /// <param name="unit">The individual combat unit which needs to be paired with this behavior.</param>
        /// <param name="supervisor">The SquadSupervisor which controlls the combat unit.</param>
        /// <param name="trainingMode">Determining if the whole flow is about executing a neural net or training one.</param>
        public CombatUnitTrainingBehavior(Unit unit, SquadSupervisor supervisor, bool trainingMode)
        {
            this.unit = unit;
            this.initialHitPoints = unit.HitPoints;
            this.squadSupervisor = supervisor;
            this.neuralNetController = NeuralNetController.GetInstance();
            this.neuralNet = neuralNetController.GetNeuralNet();
            this.trainingMode = trainingMode;
        }
        #endregion

        #region Public Functions
        /// <summary>
        /// Request the most recent global input information and complete it with local data
        /// </summary>
        /// <returns>After completing the data, the data gets returned</returns>
        public InputInformation GenerateInputInfo()
        {
            InputInformation info = squadSupervisor.GetGlobalInputInformation();
            info.CompleteInputData(unit.HitPoints, initialHitPoints);
            return info;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Returns the paired unit to this behavior class.</returns>
        public Unit GetUnit()
        {
            return this.unit;
        }
        #endregion

        #region Behavior Logic
        /// <summary>
        /// OnFrame is triggered by the SquadSupervirsor each frame. It processes the state machine for the actions and the decision making by the neural network.
        /// It also covers essential logics for generating training examples.
        /// </summary>
        public void OnFrame()
        {
            // requestDecision is used to limit the state execution. For example, an attack action needs 3 frames in order to be executed properly.
            if (requestDecision && squadSupervisor.GetEnemyCount() > 0)
            {
                MakeDecision();
            }
            else if (squadSupervisor.GetEnemyCount() == 0)
            {
                currentState = CombatUnitState.SquadState;
            }

            // state execution
            switch (currentState)
            {
                case CombatUnitState.AttackClosest:
                    AttackClosest();
                    break;
                case CombatUnitState.MoveTowards:
                    MoveTowards();
                    break;
                case CombatUnitState.MoveBack:
                    MoveBack();
                    break;
                case CombatUnitState.SquadState:
                    SquadState();
                    break;
                case CombatUnitState.Retreat:
                    Retreat();
                    break;
            }

            // Debug Visualization
            DrawUnitInfo();
        }

        /// <summary>
        /// Executes the decision process by the neural network. In the context of generating training data, this function also handles gathering data for fitness logic.
        /// </summary>
        private void MakeDecision()
        {
            InputInformation inputInfo = GenerateInputInfo();   // request input information
            double[] inputData = inputInfo.GetNormalizedData(); // normalize data
            CombatUnitState newState = (CombatUnitState)GeneralUtil.randomNumberGenerator.Next(3);      // determine random action

            if (trainingMode)
            {
                IMLData outData = neuralNet.Compute(new BasicMLData(inputData));            // compute output of the neural net
                fitnessMeasure = new FitnessMeasure(inputInfo, newState, outData);          // initialize FitnessMeasure
            }
            else
            {
                PersistenceUtil.WriteLine(neuralNet.Compute(new BasicMLData(inputData)).ToString());
                newState = (CombatUnitState)neuralNet.Classify(new BasicMLData(inputData)); // override random decision and let the net make the decision
            }

            // determine if a state transition is occuring
            if (newState != currentState)
            {
                stateTransition = true;
                currentState = newState; // transition to new state
            }
            else
            {
                stateTransition = false;
            }

            requestDecision = false;
        }

        public void AttackAction()
        {
            // Choose a new target based on a stateTransition or on the existence of the target
            if (stateTransition || currentTarget == null)
            {
                currentTarget = squadSupervisor.GetClosestEnemyUnit(this);
            }

            // check if the attack animation has been completely executed
            if (stateFrameCount < attackAnimationTime)
            {
                SmartAttack(currentTarget);
                if (unit.IsInWeaponRange(currentTarget) && unit.GroundWeaponCooldown == 0)
                {
                    stateFrameCount++; // if the unit is in range and if the weapon isn't on cooldown, count the animation frames
                }
            }
            else
            {
                stateFrameCount = 0;
                requestDecision = true;
                if (trainingMode)
                {
                    //neuralNetController.AddTrainingDataPair(fitnessMeasure.ComputeDataPair(GenerateInputInfo()));
                    squadSupervisor.FindEnemyUnitBehavior(unit.Target).OnAttackLaunched(fitnessMeasure, this);
                }
            }
        }

        /// <summary>
        /// The initial state is idle. Basically the combat unit will wait for the neural net to take over which is commanded by the SquadSupervisor.
        /// </summary>
        private void SquadState()
        {
            if (unit.LastCommand.Type != UnitCommandType.HoldPosition)
            {
                unit.HoldPosition(false);
            }
        }

        /// <summary>
        /// Attack the closest enemy unit.
        /// </summary>
        private void AttackClosest()
        {
            if (stateFrameCount < 4)
            {
                Unit target = squadSupervisor.GetClosestEnemyUnit(this);
                SmartAttack(target);
                if (unit.IsInWeaponRange(target) && unit.GroundWeaponCooldown == 0)
                {
                    stateFrameCount++;
                }
            }
            else
            {
                stateFrameCount = 0;
                requestDecision = true;
                if (trainingMode)
                {
                    //neuralNetController.AddTrainingDataPair(fitnessMeasure.ComputeDataPair(GenerateInputInfo()));
                    //squadSupervisor.FindEnemyUnitBehavior(unit.Target).OnAttackLaunched(fitnessMeasure, this);
                }
            }
        }

        /// <summary>
        /// Move towards the center of the enemy's squad.
        /// </summary>
        private void MoveTowards()
        {
            if (stateFrameCount < 7)
            {
                SmartMove(squadSupervisor.GetClosestEnemyUnit(this).Position * 2);
                stateFrameCount++;
            }
            else
            {
                stateFrameCount = 0;
                requestDecision = true;
                if (trainingMode)
                {
                    //neuralNetController.AddTrainingDataPair(fitnessMeasure.ComputeDataPair(GenerateInputInfo()));
                }
            }
        }

        /// <summary>
        /// Back up from enemy.
        /// </summary>
        private void MoveBack()
        {
            if (stateFrameCount < 7)
            {
                Position closestEnemyPos = squadSupervisor.GetClosestEnemyUnit(this).Position;
                if (closestEnemyPos != null)
                {
                    Position pos = closestEnemyPos - unit.Position;
                    SmartMove((pos * -2) + unit.Position);
                }
                stateFrameCount++;
            }
            else
            {
                stateFrameCount = 0;
                requestDecision = true;
                if (trainingMode)
                {
                    //neuralNetController.AddTrainingDataPair(fitnessMeasure.ComputeDataPair(GenerateInputInfo()));
                }
            }
        }

        /// <summary>
        /// Retreat from combat.
        /// </summary>
        private void Retreat()
        {

        }
        #endregion

        #region Local Functions
        /// <summary>
        /// SmartAttack prevents issuing the same command over and over again.
        /// </summary>
        /// <param name="target">The unit to attack.</param>
        private void SmartAttack(Unit target)
        {
            // return, if the target is null or dead
            if (target == null || target.HitPoints == 0)
            {
                return;
            }

            // Draw attack line
            Game.DrawLineMap(unit.Position.X, unit.Position.Y, target.Position.X, target.Position.Y, Color.Green);

            // check if the unit is already attacking the target, so that the same command won't be issued over and over again
            if (unit.LastCommand.Type == UnitCommandType.AttackUnit && unit.LastCommand.Target == target)
            {
                return;
            }

            unit.Attack(target, false);
        }

        /// <summary>
        /// SmartMove prevents issuing the same command over and over again.
        /// </summary>
        /// <param name="targetPosition">The position to move to.</param>
        private void SmartMove(Position targetPosition)
        {
            // if the position is null or isn't valide, return
            if (targetPosition.IsInvalid || targetPosition == null)
            {
                return;
            }

            // check if the last command is equal to the current command, same applies to the target
            if (unit.LastCommand.Type == UnitCommandType.Move && unit.LastCommand.TargetPosition == targetPosition && unit.IsMoving)
            {
                return;
            }

            // execute movement
            if(!unit.Move(targetPosition, false))
            {
                unit.HoldPosition(false);
            }
        }

        /// <summary>
        /// Draws information close to the unit for debugging purposes. Displays the current state of the unit.
        /// </summary>
        private void DrawUnitInfo()
        {
            Game.DrawTextMap(unit.Position.X, unit.Position.Y, "{0}", currentState.ToString());
            if (unit.IsStimmed)
            {
                Game.DrawCircleMap(unit.Position.X, unit.Position.Y + 12, 2, Color.Red, true);
            }
        }
        #endregion
    }
}
