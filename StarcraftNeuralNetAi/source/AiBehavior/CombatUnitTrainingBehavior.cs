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
        private SquadSupervisor squadSupervisor;
        private NeuralNetController neuralNetController;
        private BasicNetwork neuralNet;
        private FitnessMeasure fitnessMeasure;
        private bool requestDecision = true;
        private CombatUnitState currentState = CombatUnitState.SquadState;
        private bool stateTransition = true;
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
            this.squadSupervisor = supervisor;
            // Load the artificial neural network
            this.neuralNetController = NeuralNetController.GetInstance();
            this.neuralNet = neuralNetController.GetNeuralNet();
            this.trainingMode = trainingMode;
        }
        #endregion

        #region Public Functions
        // Getter
        public Unit GetUnit()
        {
            return this.unit;
        }

        public SquadSupervisor GetSupervisor()
        {
            return this.squadSupervisor;
        }

        public CombatUnitState GetCurrentState()
        {
            return this.currentState;
        }
        #endregion

        #region Behavior Logic
        /// <summary>
        /// This function is triggered by the SquadSupervirsor each frame. It processes the state machine for the actions and the decision making by the neural network.
        /// </summary>
        public void ExecuteStateMachine()
        {
            // requestDecision is used to limit the state execution. For example, an attack action needs 3 frames in order to be executed properly.
            if (requestDecision)
            {
                InputInformation inputInfo = GenerateInputInfo();   // request input information
                double[] inputData = inputInfo.GetNormalizedData(); // normalize data
                CombatUnitState newState = (CombatUnitState)GeneralUtil.randomNumberGenerator.Next(6);      // determine random actio

                if (trainingMode)
                {
                    IMLData outData = neuralNet.Compute(new BasicMLData(inputData));            // compute output of the neural net
                    fitnessMeasure = new FitnessMeasure(inputInfo, newState, outData);          // initialize FitnessMeasure
                }
                else
                {
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

            // state execution
            switch (currentState)
            {
                case CombatUnitState.AttackClosest:
                    AttackClosest();
                    break;
                case CombatUnitState.AttackStrongest:
                    AttackStrongest();
                    break;
                case CombatUnitState.AttackWeakest:
                    AttackWeakest();
                    break;
                case CombatUnitState.MoveTowards:
                    MoveTowards();
                    break;
                case CombatUnitState.MoveBack:
                    MoveBack();
                    break;
                case CombatUnitState.UseStimpack:
                    UseStimpack();
                    break;
                case CombatUnitState.SquadState:
                    SquadState();
                    break;
                case CombatUnitState.Retreat:
                    Retreat();
                    break;
            }

            // Some Debug Visualization
            DrawUnitInfo();
        }

        /// <summary>
        /// The initial state is idle. Basically the combat unit will wait for the neural net to take over which is commanded by the SquadSupervisor.
        /// </summary>
        private void SquadState()
        {

        }

        /// <summary>
        /// Attack the closest enemy unit.
        /// </summary>
        private void AttackClosest()
        {
            if (stateFrameCount < 4)
            {
                SmartAttack(squadSupervisor.GetClosestEnemyUnit(this));
                stateFrameCount++;
            }
            else
            {
                stateFrameCount = 0;
                requestDecision = true;
                if (trainingMode)
                {
                    neuralNetController.AddTrainingDataPair(fitnessMeasure.ComputeDataPair(GenerateInputInfo()));
                }
            }
        }

        /// <summary>
        /// Attack the strongest enemy unit.
        /// </summary>
        private void AttackStrongest()
        {
            if (stateFrameCount < 4)
            {
                SmartAttack(squadSupervisor.GetStrongestEnemyUnit());
                stateFrameCount++;
            }
            else
            {
                stateFrameCount = 0;
                requestDecision = true;
                if (trainingMode)
                {
                    neuralNetController.AddTrainingDataPair(fitnessMeasure.ComputeDataPair(GenerateInputInfo()));
                }
            }
        }

        /// <summary>
        /// Attack the weakest enemy unit. Being weak depends on all the properties of a unit. So a valueable unit being low on health is considered to be weak compared to a cheap unit with full health.
        /// </summary>
        private void AttackWeakest()
        {
            if (stateFrameCount < 4)
            {
                SmartAttack(squadSupervisor.GetWeakestEnemyUnit());
                stateFrameCount++;
            }
            else
            {
                stateFrameCount = 0;
                requestDecision = true;
                if (trainingMode)
                {
                    neuralNetController.AddTrainingDataPair(fitnessMeasure.ComputeDataPair(GenerateInputInfo()));
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
                SmartMove(squadSupervisor.GetEnemySquadCenter());
                stateFrameCount++;
            }
            else
            {
                stateFrameCount = 0;
                requestDecision = true;
                if (trainingMode)
                {
                    neuralNetController.AddTrainingDataPair(fitnessMeasure.ComputeDataPair(GenerateInputInfo()));
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
                Position enemySquadPos = squadSupervisor.GetEnemySquadCenter();
                if (enemySquadPos != null)
                {
                    Position pos = squadSupervisor.GetEnemySquadCenter() - unit.Position;
                    SmartMove((pos * -1) + unit.Position);
                }
                stateFrameCount++;
            }
            else
            {
                stateFrameCount = 0;
                requestDecision = true;
                if (trainingMode)
                {
                    neuralNetController.AddTrainingDataPair(fitnessMeasure.ComputeDataPair(GenerateInputInfo()));
                }
            }
        }

        /// <summary>
        /// Use stimpack. That's basically a one frame command.
        /// </summary>
        private void UseStimpack()
        {
            if (stateFrameCount < 1)
            {
                if (!unit.IsStimmed)
                {
                    unit.UseTech(new Tech(TechType.Stim_Packs.GetHashCode()));
                }
                stateFrameCount++;
            }
            else
            {
                stateFrameCount = 0;
                requestDecision = true;
                if (trainingMode)
                {
                    neuralNetController.AddTrainingDataPair(fitnessMeasure.ComputeDataPair(GenerateInputInfo()));
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
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private InputInformation GenerateInputInfo()
        {
            // request most recent global input information
            InputInformation info = squadSupervisor.GetGlobalInputInformation();

            // complete the inputInfo with local information
            if (unit.IsStimmed)
            {
                info.CompleteInputData(unit.HitPoints, 0.75);
            }
            else
            {
                info.CompleteInputData(unit.HitPoints, 0.375);
            }

            return info;
        }
        #endregion
    }
}
