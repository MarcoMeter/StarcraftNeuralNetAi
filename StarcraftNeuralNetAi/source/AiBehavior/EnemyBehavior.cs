using BroodWar.Api;

namespace NeuralNetTraining
{
    /// <summary>
    /// Each EnemyBehavior object is related to an enemy combat unit. It adds more logic and properties to an enemy unit.
    /// For now it helps to determine the alive state of that particular unit.
    /// </summary>
    public class EnemyBehavior
    {
        #region Members Fields
        // Unit related props
        private Unit m_unit;
        private int m_hitPoints;
        private bool m_isAlive = true;
        private SquadSupervisor m_squadSupervisor;

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
        public EnemyBehavior(Unit unit, SquadSupervisor supervisor)
        {
            this.m_unit = unit;
            this.m_hitPoints = unit.HitPoints;
            this.m_squadSupervisor = supervisor;
            this.m_trainingMode = TrainingModule.TrainingMode;
        }
        #endregion

        #region Behavior Logic
        /// <summary>
        /// OnFrame is triggered each frame by the SquadSupervisor for further logic.
        /// </summary>
        public void OnFrame()
        {
        }
        #endregion
    }
}
