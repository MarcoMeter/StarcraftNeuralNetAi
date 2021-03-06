﻿using NeuralNetTraining.Utility;
using BroodWar.Api;

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
        private double m_weakestEnemyHitPoints;

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
        private double m_cloestsEnemyHitPoints;
        private double m_weakestEnemyDistance;
        private double m_underAttack;

        // Information used for normalization (overall is known by SquadSupervisor)
        private double m_overAllInitialFriendlyHitPoints;
        private double m_overAllInitialFriendlyCount;
        private double m_overAllInitialEnemyHitPoints;
        private double m_overAllInitialEnemyCount;
        private double m_unitInitialHitPoints;
        private const double m_distanceOperator = 1000;

        private bool m_isCompleted = false; // the SquadSUpervisor initializes this object, the missing information is contributed by the individual unit
        private int m_frameOfCompletion;
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

        /// <summary>
        /// Read-only time stamp.
        /// </summary>
        public int FrameOfCompletion
        {
            get
            {
                return this.m_frameOfCompletion;
            }
        }

        public double OverAllFriendlyHitPoints
        {
            get
            {
                return m_overAllFriendlyHitPoints / m_overAllInitialFriendlyHitPoints;
            }
        }

        public double OverAllFriendlyCount
        {
            get
            {
                return m_overAllFriendlyCount / m_overAllInitialFriendlyCount;
            }
        }

        public double OverAllEnemyHitPoints
        {
            get
            {
                return m_overAllEnemyHitPoints / m_overAllInitialEnemyHitPoints;
            }
        }

        public double OverAllEnemyCount
        {
            get
            {
                return m_overAllEnemyCount / m_overAllInitialEnemyCount;
            }
        }

        public double CloseRangeFriendlyHitPoints
        {
            get
            {
                return m_closeRangeFriendlyHitPoints / m_overAllInitialFriendlyHitPoints;
            }
        }

        public double CloseRangeFriendlyCount
        {
            get
            {
                return m_closeRangeFriendlyCount / m_overAllInitialFriendlyCount;
            }
        }

        public double CloseRangeEnemyHitPoints
        {
            get
            {
                return m_closeRangeEnemyHitPoints / m_overAllInitialEnemyHitPoints;
            }
        }


        public double CloseRangeEnemyCount
        {
            get
            {
                return m_closeRangeEnemyCount / m_overAllInitialEnemyCount;
            }
        }

        public double FarRangeFriendlyHitPoints
        {
            get
            {
                return m_farRangeFriendlyHitPoints / m_overAllInitialFriendlyHitPoints;
            }
        }

        public double FarRangeFriendlyCount
        {
            get
            {
                return m_farRangeFriendlyCount / m_overAllInitialFriendlyCount;
            }
        }

        public double FarRangeEnemyHitPoints
        {
            get
            {
                return m_farRangeEnemyHitPoints / m_overAllInitialEnemyHitPoints;
            }
        }

        public double FarRangeEnemyCount
        {
            get
            {
                return m_farRangeEnemyCount / m_overAllInitialEnemyCount;
            }
        }

        public double UnitHitPoints
        {
            get
            {
                return m_unitHitPoints / m_unitInitialHitPoints;
            }
        }

        public double ClosestEnemyDistance
        {
            get
            {
                return m_closestEnemyDistance / m_distanceOperator;
            }
        }

        public double WeakestEnemyHitPoints
        {
            get
            {
                return m_weakestEnemyHitPoints / m_unitInitialHitPoints;
            }
        }

        public double WeakestEnemyDistance
        {
            get
            {
                return m_weakestEnemyDistance / m_distanceOperator;
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
        public InputVector(GlobalInputInfo globalInputInfo)
        {
            this.m_overAllFriendlyHitPoints = globalInputInfo.overAllFriendlyHitPoints;
            this.m_overAllFriendlyCount = globalInputInfo.overAllFriendlyCount;
            this.m_overAllEnemyHitPoints = globalInputInfo.overAllEnemyHitPoints;
            this.m_overAllEnemyCount = globalInputInfo.overAllEnemyCount;
            this.m_overAllInitialFriendlyHitPoints = globalInputInfo.overAllInitialFriendlyHitPoints;
            this.m_overAllInitialFriendlyCount = globalInputInfo.overAllInitialFriendlyCount;
            this.m_overAllInitialEnemyHitPoints = globalInputInfo.overAllInitialEnemyHitPoints;
            this.m_overAllInitialEnemyCount = globalInputInfo.overAllInitialEnemyCount;
            this.m_weakestEnemyHitPoints = globalInputInfo.weakestEnemyHitPoints;
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
                                      int frFriendlyHitPoints, int frFriendlyCount, int frEnemyHitPoints, int frEnemyCount, double closestEnemyDistance, double closestEnemyHitPoints, double weakestEnemyDistance,
                                      bool isUnderAttack)
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
            this.m_cloestsEnemyHitPoints = closestEnemyHitPoints;
            this.m_weakestEnemyDistance = weakestEnemyDistance;
            if(isUnderAttack)
            {
                m_underAttack = 1;
            }
            else
            {
                m_underAttack = 0;
            }
            this.m_isCompleted = true;
            this.m_frameOfCompletion = Game.FrameCount;
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
                m_closestEnemyDistance / m_distanceOperator,
                m_cloestsEnemyHitPoints / m_unitInitialHitPoints,
                m_weakestEnemyHitPoints / m_unitInitialHitPoints,
                m_weakestEnemyDistance / m_distanceOperator,
                m_underAttack
            };
        }
        #endregion
    }
}