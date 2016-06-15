using NeuralNetTraining.Utility;

namespace NeuralNetTraining
{
    /// <summary>
    /// This object stores and normalizes information for the input layer of the neural network.
    /// </summary>
    public class InputVector
    {
        #region Member Fields
        // Squad Supervisor knows the over all information
        private double m_overAllFriendlyHitPoints;
        private double m_overAllFriendlyCount;
        private double m_overAllEnemyHitPoints;
        private double m_overAllEnemyCount;

        // Individual unit informaiton
        private double m_unitHitPoints;
        private double m_closeRangeFriendlyHitPoints;
        private double m_closeRangeFriendlyCount;
        private double m_closeRangeEnemyHitPoints;
        private double m_closeRangeEnemyCount;
        private double m_farRangeFriendlyHitPoints;
        private double m_farRangeFriendlyCount;
        private double m_farRangeEnemyHitPoints;
        private double m_farRangeEnemyCount;
        private double m_closestEnemyDistance;

        // Information used for normalization (overall is known by SquadSupervisor)
        private double m_overAllInitialFriendlyHitPoints;
        private double m_overAllInitialFriendlyCount;
        private double m_overAllInitialEnemyHitPoints;
        private double m_overAllInitialEnemyCount;
        private double m_unitInitialHitPoints;
        private const double m_distanceOperator = 1000;

        private bool m_isCompleted = false; // the SquadSUpervisor initializes this object, the missing information is contributed by the individual unit
        #endregion

        #region Member Properties
        /// <summary>
        /// Read-only, state if the input information is completed.
        /// </summary>
        public bool IsCompleted
        {
            get
            {
                return this.m_isCompleted;
            }
        }

        public double OverAllFriendlyHitPoints
        {
            get
            {
                return m_overAllFriendlyHitPoints;
            }
        }

        public double OverAllFriendlyCount
        {
            get
            {
                return m_overAllFriendlyCount;
            }
        }

        public double OverAllEnemyHitPoints
        {
            get
            {
                return m_overAllEnemyHitPoints;
            }
        }

        public double OverAllEnemyCount
        {
            get
            {
                return m_overAllEnemyCount;
            }
        }

        public double CloseRangeFriendlyHitPoints
        {
            get
            {
                return m_closeRangeFriendlyHitPoints;
            }
        }

        public double CloseRangeFriendlyCount
        {
            get
            {
                return m_closeRangeFriendlyCount;
            }
        }

        public double CloseRangeEnemyHitPoints
        {
            get
            {
                return m_closeRangeEnemyHitPoints;
            }
        }


        public double CloseRangeEnemyCount
        {
            get
            {
                return m_closeRangeEnemyCount;
            }
        }

        public double FarRangeFriendlyHitPoints
        {
            get
            {
                return m_farRangeFriendlyHitPoints;
            }
        }

        public double FarRangeFriendlyCount
        {
            get
            {
                return m_farRangeFriendlyCount;
            }
        }

        public double FarRangeEnemyHitPoints
        {
            get
            {
                return m_farRangeEnemyHitPoints;
            }
        }

        public double FarRangeEnemyCount
        {
            get
            {
                return m_farRangeEnemyCount;
            }
        }

        public double UnitHitPoints
        {
            get
            {
                return m_unitHitPoints;
            }
        }

        public double ClosestEnemyDistance
        {
            get
            {
                return m_closestEnemyDistance;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes the input vector with information of the SquadSupervisor.
        /// </summary>
        /// <param name="friendlyHitPoints">overall friendly hit points</param>
        /// <param name="friendlyCount">overall friendly count</param>
        /// <param name="enemyHitPoints">overall enemy hit points</param>
        /// <param name="enemyCount">overall enemy count</param>
        /// <param name="initialFriendlyHitPoints">overall and initial</param>
        /// <param name="initialFriendlyCount">overall and initial</param>
        /// <param name="initialEnemyHitPoints">overall and initial</param>
        /// <param name="initialEnemyCount">overall and initial</param>
        public InputVector(int friendlyHitPoints, int friendlyCount, int enemyHitPoints, int enemyCount,
                           int initialFriendlyHitPoints, int initialFriendlyCount, int initialEnemyHitPoints, int initialEnemyCount)
        {
            this.m_overAllFriendlyHitPoints = friendlyHitPoints;
            this.m_overAllFriendlyCount = friendlyCount;
            this.m_overAllEnemyHitPoints = enemyHitPoints;
            this.m_overAllEnemyCount = enemyCount;
            this.m_overAllInitialFriendlyHitPoints = initialFriendlyHitPoints;
            this.m_overAllInitialFriendlyCount = initialFriendlyCount;
            this.m_overAllInitialEnemyHitPoints = initialEnemyHitPoints;
            this.m_overAllInitialEnemyCount = initialEnemyCount;
        }
        #endregion

        #region Public Functions
        /// <summary>
        /// After the SquadSupervisor initialized the global information, the individual CombatUnitTrainingBehavior has to add its local information in order to complete the InputVector.
        /// </summary>
        /// <param name="hitPoints">local unit's hit points</param>
        /// <param name="crFriendlyHitPoints">hit points of all friendly units in close range</param>
        /// <param name="crFriendlyCount">number of all friendly units in close range</param>
        /// <param name="crEnemyHitPoints">hit points of all enemy units in close range</param>
        /// <param name="crEnemyCount">number of all enemy units in close range</param>
        /// <param name="frFriendlyHitPoints">hit points of all friendly units in far range</param>
        /// <param name="frFriendlyCount">number of all friendly units in far range</param>
        /// <param name="frEnemyHitPoints">hit points of all enemy units in far range</param>
        /// <param name="frEnemyCount">number of all enemy units in far range</param>
        /// <param name="closestEnemyDistance"></param>
        public void CompleteInputData(int hitPoints, int initialHitPoints, int crFriendlyHitPoints, int crFriendlyCount, int crEnemyHitPoints, int crEnemyCount,
                                      int frFriendlyHitPoints, int frFriendlyCount, int frEnemyHitPoints, int frEnemyCount, double closestEnemyDistance)
        {
            this.m_unitHitPoints = hitPoints;
            this.m_unitInitialHitPoints = initialHitPoints;
            this.m_closeRangeFriendlyHitPoints = crFriendlyHitPoints;
            this.m_closeRangeFriendlyCount = crFriendlyCount;
            this.m_closeRangeEnemyHitPoints = crEnemyHitPoints;
            this.m_closeRangeEnemyCount = crEnemyCount;
            this.m_farRangeFriendlyHitPoints = frFriendlyHitPoints;
            this.m_farRangeFriendlyCount = frFriendlyCount;
            this.m_farRangeEnemyHitPoints = frEnemyHitPoints;
            this.m_farRangeEnemyCount = frEnemyCount;
            this.m_closestEnemyDistance = closestEnemyDistance;
            this.m_isCompleted = true;
        }

        /// <summary>
        /// Normalize the stored input information, so that it can be processed efficiently by the neural net.
        /// </summary>
        /// <returns>Input vector: Normalized array of input information for the neural net.</returns>
        public double[] GetNormalizedData()
        {
            return new double[]
            {
                m_unitHitPoints / m_unitInitialHitPoints,
                m_overAllFriendlyHitPoints / m_overAllInitialFriendlyHitPoints,
                m_overAllFriendlyCount / m_overAllInitialFriendlyCount,
                m_overAllEnemyHitPoints / m_overAllInitialEnemyHitPoints,
                m_overAllEnemyCount / m_overAllInitialEnemyCount,
                m_closeRangeFriendlyHitPoints / m_overAllInitialFriendlyHitPoints,
                m_closeRangeFriendlyCount / m_overAllInitialFriendlyCount,
                m_closeRangeEnemyHitPoints / m_overAllInitialEnemyHitPoints,
                m_closeRangeEnemyCount / m_overAllInitialEnemyCount,
                m_farRangeFriendlyHitPoints / m_overAllInitialFriendlyHitPoints,
                m_farRangeFriendlyCount / m_overAllInitialFriendlyCount,
                m_farRangeEnemyHitPoints / m_overAllInitialEnemyHitPoints,
                m_farRangeEnemyCount / m_overAllInitialEnemyCount,
                m_closestEnemyDistance / m_distanceOperator
            };
        }
        #endregion

        #region Local Functions
        #endregion
    }
}