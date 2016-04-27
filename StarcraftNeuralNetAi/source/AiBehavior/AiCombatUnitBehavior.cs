using BroodWar;
using BroodWar.Api;
using BroodWar.Api.Enum;
using Encog.ML.Data;
using Encog.ML.Data.Basic;
using Encog.Neural.Networks;
using Encog.Persist;
using System;
using System.IO;
using System.Drawing;

namespace NetworkTraining
{
    /// <summary>
    /// Each controlled combat unit should come with an AiCombatUnitBehavior in order to attach a neural net behavior to that combat unit.
    /// </summary>
    public class AiCombatUnitBehavior
    {
        #region Member
        private Unit unit;
        private SquadSupervisor squadSupervisor;
        private CombatUnitState currentState = CombatUnitState.SquadState;
        private bool stateTransition = true;
        private InputInformation inputInfo;
        private BasicNetwork neuralNet;
        private bool requestDecision = true;
        private int stateFrameCount = 0;
        #endregion

        #region Constructor
        /// <summary>
        /// Create an instance of the AiCombatBehavior.
        /// </summary>
        /// <param name="unit">The individual combat unit which needs to be pared with this behavior.</param>
        /// <param name="supervisor">The SquadSupervisor which controlls the combat unit.</param>
        public AiCombatUnitBehavior(Unit unit, SquadSupervisor supervisor)
        {
            this.unit = unit;
            this.squadSupervisor = supervisor;
            // Load the artificial neural network
            this.neuralNet = (BasicNetwork)EncogDirectoryPersistence.LoadObject(new FileInfo("testNetwork" + Game.Self.Id.ToString() + ".ann"));
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

        public InputInformation GetInputInformation()
        {
            return this.inputInfo;
        }

        // Setter
        public void ForceState(CombatUnitState state)
        {
            this.currentState = state;
        }
        #endregion

        #region Behavior Logic
        /// <summary>
        /// The neural net will be executed several times a second, which is determined by the SquadSupervisor. Based on the decisions made by the net, the concerning state of the finite state machine will be executed.
        /// </summary>
        public void ExecuteStateMachine()
        {
            // requestDecision is used to limit the state execution. For example, an attack action needs 3 frames in order to be executed properly.
            if (requestDecision)
            {
                inputInfo = squadSupervisor.GetGlobalInputInformation(); // request most recent global input information

                // Complete the inputInfo with local information
                // Pure damage is not considered to be as an input, just because the damage is considered to be constant for now. Weapon cooldown is much more interesting due to stimpack and timing.
                inputInfo.CompleteInputData(unit.HitPoints, unit.GroundWeaponCooldown, unit.VelocityX, unit.VelocityY, unit.IsStimmed);
                double[] inputData = inputInfo.GetNormalizedData();

                // Use the neural net to classify the input information
                //CombatUnitState newState = (CombatUnitState)neuralNet.Classify(new BasicMLData(inputData));
                

                // Random decision
                CombatUnitState newState;
                newState = (CombatUnitState)Utility.randomNumberGenerator.Next(6);

                // Determine if a state transition occured
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
            Game.Write(currentState.ToString());

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

            // fitness function?

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
            else if (stateFrameCount >= 4 || unit.GroundWeaponCooldown > 0)
            {
                stateFrameCount = 0;
                requestDecision = true;
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
            else if(stateFrameCount >= 4 || unit.GroundWeaponCooldown > 0)
            {
                stateFrameCount = 0;
                requestDecision = true;
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
            else if (stateFrameCount >= 4 || unit.GroundWeaponCooldown > 0)
            {
                stateFrameCount = 0;
                requestDecision = true;
            }
        }

        /// <summary>
        /// Move towards the center of the enemy's squad.
        /// </summary>
        private void MoveTowards()
        {
            if (stateFrameCount < 10)
            {
                SmartMove(squadSupervisor.GetEnemySquadCenter());
                stateFrameCount++;
            }
            else
            {
                stateFrameCount = 0;
                requestDecision = true;
            }
        }

        /// <summary>
        /// Back up from enemy.
        /// </summary>
        private void MoveBack()
        {
            if (stateFrameCount < 10)
            {
                Position enemySquadPos = squadSupervisor.GetEnemySquadCenter();
                Position pos = squadSupervisor.GetEnemySquadCenter() - unit.Position;
                SmartMove((pos * -1) + unit.Position);
                stateFrameCount++;
            }
            else
            {
                stateFrameCount = 0;
                requestDecision = true;
            }
        }

        /// <summary>
        /// Use stimpack. That's basically a one frame command.
        /// </summary>
        private void UseStimpack()
        {
            if (!unit.IsStimmed)
            {
                unit.UseTech(new Tech(TechType.Stim_Packs.GetHashCode()));
            }

            stateFrameCount = 0;
            requestDecision = true;
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
            if (target == null)
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
            if (targetPosition.IsInvalid)
            {
                return;
            }

            if (unit.LastCommand.Type == UnitCommandType.Move && unit.LastCommand.TargetPosition == targetPosition && unit.IsMoving)
            {
                return;
            }

            if(!unit.Move(targetPosition, false))
            {
                unit.HoldPosition(false);
            }
        }

        /// <summary>
        /// Draws information close to the unit for debugging purposes.
        /// </summary>
        private void DrawUnitInfo()
        {
            Game.DrawTextMap(unit.Position.X, unit.Position.Y - 5, currentState.ToString(), null);
        }
        #endregion
    }
}
