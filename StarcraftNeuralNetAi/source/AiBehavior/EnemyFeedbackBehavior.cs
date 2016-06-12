using System;
using System.IO;
using System.Drawing;
using BroodWar.Api;
using BroodWar.Api.Enum;
using System.Collections.Generic;
using NeuralNetTraining.Utility;

namespace NeuralNetTraining
{
    public class EnemyFeedbackBehavior
    {
        #region Members Fields
        // Unit related props
        private Unit m_unit;
        private int m_hitPoints;
        private bool m_isAlive = true;
        private SquadSupervisor m_squadSupervisor;

        // Attack feedback
        private List<CombatUnitTrainingBehavior> m_attackers = new List<CombatUnitTrainingBehavior>();
        private List<AttackFitnessMeasure> m_attackersFitness = new List<AttackFitnessMeasure>();
        private bool m_isExpectingFeedback = false;

        private bool m_trainingMode;
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
        /// Read-only friendly SquadSupervisor.
        /// </summary>
        public SquadSupervisor SquadSupervisor
        {
            get
            {
                return this.m_squadSupervisor;
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
        /// Based on an enemy combat unit, this behavior is constructed to add more functionality and logics to a single foe.
        /// </summary>
        /// <param name="unit">Enemy unit</param>
        /// <param name="supervisor">Friendly SquadSupervisor</param>
        public EnemyFeedbackBehavior(Unit unit, SquadSupervisor supervisor)
        {
            this.m_unit = unit;
            this.m_hitPoints = unit.HitPoints;
            this.m_squadSupervisor = supervisor;
            this.m_trainingMode = TrainingModule.TrainingMode;
        }
        #endregion

        #region Public Functions
        /// <summary>
        /// A friendly unit trigger the launch of a strike or projectile and makes this enemy combat unit to await feedback.
        /// </summary>
        /// <param name="friendlyUnitBehavior">Attacking friendly unit behavior</param>
        public void QueueAttack(CombatUnitTrainingBehavior friendlyUnitBehavior)
        {
            m_attackers.Add(friendlyUnitBehavior);
        }

        #endregion

        #region Behavior Logic
        /// <summary>
        /// OnFrame is triggered each frame by the SquadSupervisor for further logic.
        /// </summary>
        public void OnFrame()
        {
            if (m_attackers.Count > 0 && m_trainingMode)
            {
                // check if damage is taken
                if (m_unit.HitPoints < m_hitPoints)
                {
                    m_attackers[0].EnemyImpactMessage();
                    m_attackers.RemoveAt(0);
                }
            }
        }
        #endregion

        #region Local Functions
        #endregion
    }
}
