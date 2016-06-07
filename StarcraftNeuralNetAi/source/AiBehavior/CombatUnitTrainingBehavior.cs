﻿using System;
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
    /// Unfortunately the unit class is sealed, which prohibites deriving from it.
    /// </summary>
    public class CombatUnitTrainingBehavior
    {
        #region Member Fields
        // Unit related props
        private Unit m_unit;
        private int m_initialHitPoints;
        private bool m_isAlive = true;

        // State and decision fields
        private SquadSupervisor m_squadSupervisor;
        private NeuralNetController m_neuralNetController;
        private BasicNetwork m_neuralNet;
        private AttackFitnessMeasure m_fitnessMeasure;
        private bool m_requestDecision = true;
        private CombatUnitState m_currentState = CombatUnitState.SquadState;
        private bool m_stateTransition = true;

        // Attack action
        private Unit m_currentTarget;
        private bool m_attackAnimProcess = false;

        // Run action
        private const int m_runDuration = 7;
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
        public CombatUnitTrainingBehavior(Unit unit, SquadSupervisor supervisor)
        {
            this.m_unit = unit;
            this.m_initialHitPoints = unit.HitPoints;
            this.m_squadSupervisor = supervisor;
            this.m_neuralNetController = NeuralNetController.Instance;
            this.m_neuralNet = m_neuralNetController.NeuralNet;
            this.m_trainingMode = TrainingModule.TrainingMode;
        }
        #endregion

        #region Public Functions
        /// <summary>
        /// Request the most recent global input information and complete it with local data
        /// </summary>
        /// <returns>After completing the data, the data gets returned</returns>
        public InputInformation GenerateInputInfo()
        {
            InputInformation info = m_squadSupervisor.GlobalInputInfo;
            info.CompleteInputData(m_unit.HitPoints, m_initialHitPoints, m_unit.Distance(m_squadSupervisor.GetClosestEnemyUnit(this)));
            return info;
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
                else if (m_currentState == CombatUnitState.RunAway)
                {
                    RunAway();
                }
                else
                {
                    AttackAction();
                }

                // Debug Visualization
                DrawUnitInfo();
            }
            else
            {
                
            }
        }

        /// <summary>
        /// Executes the decision process by the neural network. In the context of generating training data, this function also handles gathering data for fitness logic.
        /// </summary>
        private void MakeDecision()
        {
            InputInformation inputInfo = GenerateInputInfo();   // request input information
            double[] inputData = inputInfo.GetNormalizedData(); // normalize data
            CombatUnitState newState = (CombatUnitState)GeneralUtil.RandomNumberGenerator.Next(3);      // determine random action

            if (m_trainingMode)
            {
                IMLData outData = m_neuralNet.Compute(new BasicMLData(inputData));            // compute output of the neural net
                // initialize AttackFitnessMeasure with the current inputs, the random state and the computed network output
                m_fitnessMeasure = new AttackFitnessMeasure(inputInfo, newState, outData);
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
            if (m_stateTransition || m_currentTarget == null)
            {
                // Find a target based on the current state
                switch(m_currentState)
                {
                    case CombatUnitState.AttackClosest:
                        m_currentTarget = m_squadSupervisor.GetClosestEnemyUnit(this);
                        break;
                    case CombatUnitState.AttackWeakest:
                        m_currentTarget = m_squadSupervisor.GetWeakestEnemyUnit();
                        break;
                }
            }

            // Issue the attack
            SmartAttack(m_currentTarget);

            // Wait for the beginning of the attack animation, this may include getting in weapon range and waiting for the cooldown to be done
            if (m_unit.IsAttackFrame && !m_attackAnimProcess)
            {
                m_squadSupervisor.FindEnemyUnitBehavior(m_currentTarget).QueueAttack(m_fitnessMeasure, this);
                m_attackAnimProcess = true;
            }

            // Action is done as soon as the attack animation is done as well
            // If the BWAPI provided information about when the projectile or strike is launched, the action could be considered as done earlier.
            // So this Attack Action is not "frame-perfect"
            if(m_attackAnimProcess && !m_unit.IsAttackFrame)
            {
                // impact?
                
                m_attackAnimProcess = false;
                m_requestDecision = true;
                m_stateFrameCount = 0;
            }
        }

        /// <summary>
        /// Running away is like retreating from combat. During a constant amount of frames, the unit tries to increase the distance to its closest enemy.
        /// </summary>
        private void RunAway()
        {
            Unit closestFoe = m_squadSupervisor.GetClosestEnemyUnit(this);

            if(m_stateFrameCount < m_runDuration)
            {
                SmartMove((m_unit.Position - closestFoe.Position) * 2 + m_unit.Position);
                m_stateFrameCount++;
            }
            else
            {
                // TODO compute data pair logic
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
            Game.DrawTextMap(m_unit.Position.X, m_unit.Position.Y, "{0}", m_currentState.ToString());
            if (m_unit.IsStimmed)
            {
                Game.DrawCircleMap(m_unit.Position.X, m_unit.Position.Y + m_stimmedCircleOff, m_stimmedCircleRadius, Color.Red, true);
            }
        }
        #endregion
    }
}
