using System.Drawing;
using BroodWar.Api;
using BroodWar.Api.Enum;
using Encog.Neural.Networks;
using NeuralNetTraining.Utility;
using Encog.ML.Data.Basic;
using Encog.ML.Data;
using System.Collections.Generic;

namespace NeuralNetTraining
{
    /// <summary>
    /// Each controlled combat unit should be paired to a CombatUnitTrainingBehavior. This behavior takes care of the learning procedure of the individual neural networks.
    /// Unfortunately the unit class is sealed, which prohibites deriving from it.
    /// </summary>
    public class CombatUnitTrainingBehavior
    {
        #region Member Fields
        // Unit related props
        private Unit m_unit;
        private int m_id;
        private int m_initialHitPoints;
        private bool m_isAlive = true;
        private int m_killCount = 0;

        // State and decision fields
        private SquadSupervisor m_squadSupervisor;
        private NeuralNetController m_neuralNetController;
        private BasicNetwork m_neuralNet;
        private AttackFitness m_attackFitnessMeasure;
        private MovementFitness m_movementFitnessMeasure;
        private bool m_requestDecision = true;
        private CombatUnitState m_currentState = CombatUnitState.SquadState;
        private bool m_stateTransition = true;
        private const int m_outputActionsCount = 2;

        // Information gathering
        private int m_closeRangeRadius;
        private int m_farRangeRadius;
        private int m_infoRadiusTwoMultiplier = 2;

        // Attack action
        private Unit m_currentTarget;
        private bool m_attackAnimProcess = false;
        private const int m_waitForKillCountDuration = 6;
        private KillCountHelper m_killCountHelper;

        // Run action
        private const int m_runDuration = 10;
        private int m_stateFrameCount = 0;

        // Training concerns
        private bool m_trainingMode;

        // Visual Debugging
        private const int m_stimmedCircleOff = 12;
        private const int m_stimmedCircleRadius = 2;
        #endregion

        #region Member Properties
        /// <summary>
        /// Read-only unit for the behavior's specific unit.
        /// </summary>
        public Unit Unit
        {
            get
            {
                return this.m_unit;
            }
        }

        /// <summary>
        /// Read-only id of the unit. It's usefull for debugging purposes.
        /// </summary>
        public int Id
        {
            get
            {
                return this.m_id;
            }
        }

        /// <summary>
        /// Sets and gets the alive state of the unit.
        /// </summary>
        public bool IsAlive
        {
            get
            {
                return this.m_isAlive;
            }

            set
            {
                this.m_isAlive = value;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Create an instance of the CombatUnitTrainingBehavior.
        /// </summary>
        /// <param name="unit">The individual combat unit which needs to be paired with this behavior.</param>
        /// <param name="supervisor">The SquadSupervisor which controlls the combat unit.</param>
        /// <param name="id">Assign unique Id</param>
        public CombatUnitTrainingBehavior(Unit unit, SquadSupervisor supervisor, int id)
        {
            this.m_unit = unit;
            this.m_id = id;
            this.m_initialHitPoints = unit.HitPoints;
            this.m_squadSupervisor = supervisor;
            this.m_neuralNetController = NeuralNetController.Instance;
            this.m_neuralNet = m_neuralNetController.NeuralNet;
            this.m_trainingMode = TrainingModule.TrainingMode;
            this.m_closeRangeRadius = unit.UnitType.GroundWeapon.MaxRange;
            this.m_farRangeRadius = unit.UnitType.GroundWeapon.MaxRange * m_infoRadiusTwoMultiplier;
        }
        #endregion

        #region Public Functions
        /// <summary>
        /// Request the most recent global input information and complete it with local data
        /// </summary>
        /// <returns>After completing the data, the data gets returned</returns>
        public InputVector GenerateInputVector()
        {
            InputVector input = m_squadSupervisor.GlobalInputVector; // Get global input information retrieved from the SquadSupervisor.

            // Gather information about units within the specified radii
            HashSet<Unit> closeRangeUnits = m_unit.UnitsInRadius(m_closeRangeRadius); // Find all units in close range
            HashSet<Unit> farRangeUnits = m_unit.UnitsInRadius(m_farRangeRadius); // Find all units in far range
            int closeRangeFriendlyHitPoints = 0;
            int closeRangeFriendlyCount = 0;
            int closeRangeEnemyHitPoints = 0;
            int closeRangeEnemyCount = 0;
            int farRangeFriendlyHitPoints = 0;
            int farRangeFriendlyCount = 0;
            int farRangeEnemyHitPoints = 0;
            int farRangeEnemyCount = 0;
            double closestEnemyDistance = m_unit.Distance(m_squadSupervisor.GetClosestEnemyUnit(this));

            // Close range units
            foreach(Unit u in closeRangeUnits)
            {
                if(u.Player.Id == Game.Self.Id) // friendly units
                {
                    closeRangeFriendlyHitPoints += u.HitPoints;
                    closeRangeFriendlyCount++;
                }
                else                 // enemy units
                {
                    closeRangeEnemyHitPoints += u.HitPoints;
                    closeRangeEnemyCount++;
                }
            }

            // Far range units
            foreach(Unit u in farRangeUnits)
            {
                if (u.Player.Id == Game.Self.Id) // friendly units
                {
                    farRangeFriendlyHitPoints += u.HitPoints;
                    farRangeFriendlyCount++;
                }
                else                 // enemy units
                {
                    farRangeEnemyHitPoints += u.HitPoints;
                    farRangeEnemyCount++;
                }
            }

            // Complete input vector
            input.CompleteInputData(m_unit.HitPoints, m_initialHitPoints, closeRangeFriendlyHitPoints, closeRangeFriendlyCount, closeRangeEnemyHitPoints, closeRangeEnemyCount, farRangeFriendlyHitPoints, 
                farRangeFriendlyCount, farRangeEnemyHitPoints, farRangeEnemyCount, closestEnemyDistance, m_squadSupervisor.GetClosestEnemyUnit(this).HitPoints, m_squadSupervisor.GetWeakestEnemyUnit().Distance(m_unit),
                m_unit.IsUnderAttack);
            return input;
        }
        #endregion

        #region Behavior Logic
        /// <summary>
        /// OnFrame is triggered by the SquadSupervirsor each frame. It processes the state machine for the actions and the decision making by the neural network.
        /// It also covers essential logics for generating training examples.
        /// </summary>
        public void OnFrame()
        {
            if (IsAlive)
            {
                // requestDecision is used to limit the state execution. For example, an attack action needs 3 frames in order to be executed properly.
                if (m_requestDecision && m_squadSupervisor.EnemyCount > 0)
                {
                    MakeDecision();
                }
                else if (m_squadSupervisor.EnemyCount == 0)
                {
                    m_currentState = CombatUnitState.SquadState;
                }

                // state execution, the states will determine which target to go for
                if (m_currentState == CombatUnitState.SquadState)
                {
                    SquadState();
                }
                else if (m_currentState == CombatUnitState.MoveBack)
                {
                    MoveBack();
                }
                else
                {
                    AttackAction();
                }

                // Debug Visualization
                DrawUnitInfo();
            }

            ProcessKillCountHelper(); // if a KillCounter is available, it will check the unit's kill count during a specific amount of frames
        }

        /// <summary>
        /// Executes the decision process by the neural network. In the context of generating training data, this function also handles gathering data for fitness logic.
        /// </summary>
        private void MakeDecision()
        {
            InputVector inputVector = GenerateInputVector();   // request input information
            double[] inputData = inputVector.GetNormalizedData(); // normalize data
            CombatUnitState newState = (CombatUnitState)GeneralUtil.RandomNumberGenerator.Next(m_outputActionsCount);      // determine random action

            if (m_trainingMode)
            {
                IMLData outData = m_neuralNet.Compute(new BasicMLData(inputData));            // compute output of the neural net
                // initialize AttackFitnessMeasure with the current inputs, the random state and the computed network output
                m_attackFitnessMeasure = new AttackFitness(inputVector, newState, outData);
                m_movementFitnessMeasure = new MovementFitness(inputVector, newState, outData);
            }
            else
            {
                PersistenceUtil.WriteLine(m_neuralNet.Compute(new BasicMLData(inputData)).ToString());
                newState = (CombatUnitState)m_neuralNet.Classify(new BasicMLData(inputData)); // override random decision and let the net make the decision
            }

            // determine if a state transition is occuring
            if (newState != m_currentState)
            {
                m_stateTransition = true;
                m_currentState = newState; // transition to new state
            }
            else
            {
                m_stateTransition = false;
            }

            m_requestDecision = false;
        }

        /// <summary>
        /// Carry out attack action depending on the current decision.
        /// </summary>
        private void AttackAction()
        {
            // Choose a new target based on a stateTransition or on the existence of the current target
            if (m_stateTransition || m_currentTarget == null || m_currentTarget.HitPoints == 0)
            {
                // Find a target based on the current state
                switch(m_currentState)
                {
                    case CombatUnitState.AttackClosest:
                        m_currentTarget = m_squadSupervisor.GetClosestEnemyUnit(this);
                        //SmartAttackMove((m_unit.Position - m_currentTarget.Position) * 2); // SmartAttackMove doesn't work reliably yet
                        break;
                    case CombatUnitState.AttackWeakest:
                        m_currentTarget = m_squadSupervisor.GetWeakestEnemyUnit();
                        //SmartAttack(m_currentTarget);
                        break;
                }
            }

            SmartAttack(m_currentTarget);

            // Wait for the beginning of the attack animation, this may include getting in weapon range and waiting for the cooldown to be done
            if (m_unit.IsAttackFrame && !m_attackAnimProcess)
            {
                m_attackAnimProcess = true;
            }

            // Action is done as soon as the attack animation is done as well
            // If the BWAPI provided information about when the projectile or strike is launched, the action could be considered as done earlier.
            // So this Attack Action is not "frame-perfect"
            if(m_attackAnimProcess && !m_unit.IsAttackFrame)
            {
                if (m_trainingMode)
                {
                    if(m_unit.KillCount > m_killCount)
                    {
                        // if the kill count got increased during the attack, feedback is already existing in order to evaluate the action.
                        m_killCount = m_unit.KillCount;
                        m_attackFitnessMeasure.ComputeDataPair(GenerateInputVector(), true, true);
                    }
                    else
                    {
                        // This case determines if a kill occured shortly after the taken action. If no kill occured, the fitness measure will assume a succesful hit but no kill as feedback.
                        m_killCountHelper = new KillCountHelper(m_unit, m_attackFitnessMeasure, GenerateInputVector(), m_waitForKillCountDuration);
                    }

                }
                m_attackAnimProcess = false;
                m_requestDecision = true;
                m_stateFrameCount = 0;
            }
        }

        /// <summary>
        /// Running away is like retreating from combat. During a constant amount of frames, the unit tries to increase the distance to its closest enemy.
        /// </summary>
        private void MoveBack()
        {
            Unit closestFoe = m_squadSupervisor.GetClosestEnemyUnit(this);

            if (m_stateFrameCount < m_runDuration)
            {
                SmartMove((m_unit.Position - closestFoe.Position) * 2 + m_unit.Position);
                m_stateFrameCount++;
            }
            else
            {
                if (m_trainingMode)
                {
                    m_movementFitnessMeasure.ComputeDataPair(GenerateInputVector(), false, false);
                }
                m_requestDecision = true;
                m_stateFrameCount = 0;
            }
        }

        /// <summary>
        /// The initial state is idle. Basically the combat unit will wait for the neural net to take over which is commanded by the SquadSupervisor.
        /// </summary>
        private void SquadState()
        {
            if (m_unit.LastCommand.Type != UnitCommandType.HoldPosition)
            {
                m_unit.HoldPosition(false);
            }
        }
        #endregion

        #region Local Functions
        /// <summary>
        /// Processes the logic of the KillCountHelper. After a specific amount of frames, the helper is supposed to check the unit's kill count to give proper feedback to the action's fitness function.
        /// That's due to the fact that hits or kills may be landed after the complete attack action.
        /// </summary>
        private void ProcessKillCountHelper()
        {
            if(m_killCountHelper != null)
            {
                if (m_killCountHelper.FrameDuration > 0)
                {
                    m_killCountHelper.DecreaseDuration(); // if frame duration is left, decrease the duration each frame
                }
                else
                {
                    m_killCountHelper.ProcessFitnessMeasure(m_unit.KillCount); // after the specific amount of time passed, let the helper check the kill count in order to finalize the fitness measure
                    m_killCountHelper = null;
                }
            }
        }

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
            Game.DrawLineMap(m_unit.Position.X, m_unit.Position.Y, target.Position.X, target.Position.Y, Color.Green);

            // check if the unit is already attacking the target, so that the same command won't be issued over and over again
            if (m_unit.LastCommand.Type == UnitCommandType.AttackUnit && m_unit.LastCommand.Target == target)
            {
                return;
            }

            m_unit.Attack(target, false);
        }

        /// <summary>
        /// Commands unit to move towards a position while attacking the closest units on the way. This function doesn't work reliably yet.
        /// </summary>
        /// <param name="targetPosition">Position of the target.</param>
        private void SmartAttackMove(Position targetPosition)
        {
            // return, if the target is null
            if (targetPosition == null)
            {
                return;
            }

            // Draw attack line
            Game.DrawLineMap(m_unit.Position.X, m_unit.Position.Y, targetPosition.X, targetPosition.Y, Color.Yellow);

            // check if the unit is already attacking the target position, so that the same command won't be issued over and over again
            if (m_unit.LastCommand.Type == UnitCommandType.AttackMove && m_unit.LastCommand.TargetPosition == targetPosition)
            {
                return;
            }

            m_unit.Attack(targetPosition, false);
        }

        /// <summary>
        /// SmartMove prevents issuing the same command over and over again.
        /// </summary>
        /// <param name="targetPosition">The position to move to.</param>
        private void SmartMove(Position targetPosition)
        {
            // if the position is null or isn't valide, return
            if (targetPosition == null || targetPosition.IsInvalid)
            {
                return;
            }

            // check if the last command is equal to the current command, same applies to the target
            if (m_unit.LastCommand.Type == UnitCommandType.Move && m_unit.LastCommand.TargetPosition == targetPosition && m_unit.IsMoving)
            {
                return;
            }

            // execute movement
            if(!m_unit.Move(targetPosition, false))
            {
                m_unit.HoldPosition(false);
            }
        }

        /// <summary>
        /// Draws information close to the unit for debugging purposes. Displays the current state of the unit.
        /// </summary>
        private void DrawUnitInfo()
        {
            Game.DrawTextMap(m_unit.Position.X, m_unit.Position.Y, "{0}", m_currentState.ToString()); // draw the unit's state
            if (m_unit.IsStimmed)
            {
                Game.DrawCircleMap(m_unit.Position.X, m_unit.Position.Y + m_stimmedCircleOff, m_stimmedCircleRadius, Color.Red, true); // draw a cricle to indicate stimpack status
            }

            if (IsAlive)
            {
                Game.DrawCircleMap(m_unit.Position.X, m_unit.Position.Y, m_closeRangeRadius, Color.White, false); // draw info gathering circle #1
                Game.DrawCircleMap(m_unit.Position.X, m_unit.Position.Y, m_farRangeRadius, Color.White, false); // draw info gathering circle #2
            }
        }
        #endregion
    }
}
