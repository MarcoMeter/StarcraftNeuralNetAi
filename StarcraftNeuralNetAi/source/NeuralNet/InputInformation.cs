using NeuralNetTraining.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeuralNetTraining
{
    /// <summary>
    /// This object stores and normalizes information for the input layer of the neural network.
    /// </summary>
    public class InputInformation
    {
        #region Member Fields
        // Known by SquadSupervisor
        // Enemy
        private double m_enemyHitPoints;
        private double m_initialEnemyHitPoints;
        private double m_enemySquadCount;
        private double m_initialEnemySquadCount;
        // Friendly
        private double m_friendlySquadHitPoints;
        private double m_initialFriendlySquadHitPoints;
        private double m_friendlySquadCount;
        private double m_initialFriendlySquadCount;
        // Known by CombatUnitTrainingBehavior
        // Friendly
        private double m_localHitPoints;
        private double m_initialLocalHitPoints;

        private bool m_isCompleted = false; // the SquadSUpervisor initializes this object, the missing information is contributed by the individual unit
        #endregion

        #region Constructor
        /// <summary>
        /// Instantiates an input information object based on global input information.
        /// </summary>
        /// <param name="enemyHp">overall enemy combat units hit points</param>
        /// <param name="initialEnemyHp">overall and initial enemy bombat units hit points</param>
        public InputInformation(int enemyHp, int initialEnemyHp, int enemySquadCount, int initialEnemySquadCount,
            int friendlySquadHp, int initialFriendlySquadHp, int friendlySuqadCount, int initialFriendlySquadCount)
        {
            this.m_enemyHitPoints = enemyHp;
            this.m_initialEnemyHitPoints = initialEnemyHp;
            this.m_enemySquadCount = enemySquadCount;
            this.m_initialEnemySquadCount = initialEnemySquadCount;
            this.m_friendlySquadHitPoints = friendlySquadHp;
            this.m_initialFriendlySquadHitPoints = initialFriendlySquadHp;
            this.m_friendlySquadCount = friendlySuqadCount;
            this.m_initialFriendlySquadCount = initialFriendlySquadCount;
        }
        #endregion

        #region Public Functions
        /// <summary>
        /// Normalize the stored input information, so that it can be processed efficiently by the neural net.
        /// </summary>
        /// <returns>Normalized array of input information for the neural net.</returns>
        public double[] GetNormalizedData()
        {
            return new double[] {m_enemyHitPoints / m_initialEnemyHitPoints, m_enemySquadCount / m_initialEnemySquadCount,
                m_friendlySquadHitPoints / m_initialFriendlySquadHitPoints, m_friendlySquadCount / m_initialFriendlySquadCount, m_localHitPoints / m_initialLocalHitPoints,};
        }

        /// <summary>
        /// After the SquadSupervisor initialized the global information, the individual CombatUnitTrainingBehavior has to add its local information in order to complete the InputInformation.
        /// </summary>
        /// <param name="hitPoints">current hit points of the friendly combat unit</param>
        /// <param name="initialHitPoints">initial hit points of the friendly combat unit</param>
        public void CompleteInputData(int hitPoints, int initialHitPoints, double closestEnemyDistance)
        {
            this.m_localHitPoints = (double)hitPoints;
            this.m_initialLocalHitPoints = (double)initialHitPoints;
            this.m_isCompleted = true;
        }

        /// <summary>
        /// Each input information, which is supposed to be friendly, is getting normalized first and then returned.
        /// </summary>
        /// <returns>Returns all normalized friendly values as array.</returns>
        public double[] GetFriendlyInfo()
        {
            return new double[] {m_localHitPoints / m_initialLocalHitPoints, m_friendlySquadCount / m_initialFriendlySquadCount};
        }

        /// <summary>
        /// Each input information, which is supposed to be of the enemy, is getting normalized first and then returned.
        /// </summary>
        /// <returns>Returns all normalized enemy values as array.</returns>
        public double[] GetEnemyInfo()
        {
            return new double[] {m_enemyHitPoints / m_initialEnemyHitPoints, m_enemySquadCount / m_initialEnemySquadCount};
        }

        public bool IsCompleted()
        {
            return m_isCompleted;
        }
        #endregion

        #region Local Functions
        #endregion
    }
}
