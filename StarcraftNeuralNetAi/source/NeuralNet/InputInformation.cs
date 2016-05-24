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
        private double m_enemyHitPoints;
        private double m_initialEnemyHitPoints;
        // Known by CombatUnitTrainingBehavior
        private double m_localHitPoints;
        private double m_initialLocalHitPoints;
        private bool m_isCompleted = false;
        #endregion

        #region Constructor
        /// <summary>
        /// Instantiates an input information object based on global input information.
        /// </summary>
        /// <param name="enemyHp">overall enemy combat units hit points</param>
        /// <param name="initialEnemyHp">overall and initial enemy bombat units hit points</param>
        public InputInformation(int enemyHp, int initialEnemyHp)
        {
            this.m_enemyHitPoints = enemyHp;
            this.m_initialEnemyHitPoints = initialEnemyHp;
        }
        #endregion

        #region Public Functions
        /// <summary>
        /// Normalize the stored input information, so that it can be processed efficiently by the neural net.
        /// </summary>
        /// <returns>Normalized array of input information for the neural net.</returns>
        public double[] GetNormalizedData()
        {
            return new double[] {m_enemyHitPoints / m_initialEnemyHitPoints, m_localHitPoints / m_initialLocalHitPoints};
        }

        /// <summary>
        /// After the SquadSupervisor initialized the global information, the individual CombatUnitTrainingBehavior has to add its local information in order to complete the InputInformation.
        /// </summary>
        /// <param name="hitPoints">current hit points of the friendly combat unit</param>
        /// <param name="initialHitPoints">initial hit points of the friendly combat unit</param>
        public void CompleteInputData(int hitPoints, int initialHitPoints)
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
            return new double[] {m_localHitPoints / m_initialLocalHitPoints};
        }

        /// <summary>
        /// Each input information, which is supposed to be of the enemy, is getting normalized first and then returned.
        /// </summary>
        /// <returns>Returns all normalized enemy values as array.</returns>
        public double[] GetEnemyInfo()
        {
            return new double[] {m_enemyHitPoints / m_initialEnemyHitPoints};
        }

        public bool IsCompleted()
        {
            return m_isCompleted;
        }

        /*
        public override String ToString()
        {
            return "Squad hp : " + squadHitPoints + ", Squad count : " + squadCount + ", Squad dpf : " + squadDpf + ", Enemy squad hp : " + enemyHitPoints + ", Enemy squad count : " + enemySquadCount +
                ", enemy squad dpf : " + enenmyDpf + ", Local hp : " + localHitPoints + ", Local dpf : " + localDpf;
        }
        */

        public override string ToString()
        {
            return "HP F: " + m_localHitPoints + "   HP E: " + m_enemyHitPoints;
        }
        #endregion

        #region Local Functions

        #endregion
    }
}
