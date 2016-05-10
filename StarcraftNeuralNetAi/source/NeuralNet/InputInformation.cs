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
        #region Member
        // Known by SquadSupervisor
        private double enemyHitPoints;
        private double initialEnemyHitPoints;
        // Known by CombatUnitTrainingBehavior
        private double localHitPoints;
        private double initialLocalHitPoints;
        private bool isCompleted = false;
        #endregion

        #region Constructor
        public InputInformation(int enemyHp, int initialEnemyHp)
        {
            this.enemyHitPoints = enemyHp;
            this.initialEnemyHitPoints = initialEnemyHp;
        }
        #endregion

        #region Public Functions
        /// <summary>
        /// This function normalizes all the stored information, so that it can be processed efficiently by the neural net.
        /// </summary>
        /// <returns>Normalized array of input information for the neural net.</returns>
        public double[] GetNormalizedData()
        {
            return new double[] {enemyHitPoints / initialEnemyHitPoints, localHitPoints / initialLocalHitPoints};
        }

        /// <summary>
        /// After the SquadSupervisor initialized the global information, the individual AiCombatUnitBehavior has to add its local information in order to complete the InputInformation.
        /// </summary>
        /// <param name="hitPoints">Remaining hit points of the combat unit.</param>
        /// <param name="weaponCooldown">Remaining cooldown till next attack.</param>
        /// <param name="velocityX">The speed of the combat unit on the x-axis.</param>
        /// <param name="velocityY">The speed of the combat unit on the y-axis.</param>
        /// <param name="isStimmed">Is the combat unit on the effect caused by the Stimpack?</param>
        public void CompleteInputData(int hitPoints, int initialHitPoints)
        {
            this.localHitPoints = (double)hitPoints;
            this.initialLocalHitPoints = (double)initialHitPoints;
            this.isCompleted = true;
        }

        // Getter

        /// <summary>
        /// Each input information, which is supposed to be friendly, is getting normalized first and then returned.
        /// </summary>
        /// <returns>Returns all normalized friendly values as array.</returns>
        public double[] GetFriendlyInfo()
        {
            return new double[] {localHitPoints / initialLocalHitPoints};
        }

        /// <summary>
        /// Each input information, which is supposed to be of the enemy, is getting normalized first and then returned.
        /// </summary>
        /// <returns>Returns all normalized enemy values as array.</returns>
        public double[] GetEnemyInfo()
        {
            return new double[] {enemyHitPoints / initialEnemyHitPoints};
        }

        public bool IsCompleted()
        {
            return isCompleted;
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
            return "HP F: " + localHitPoints + "   HP E: " + enemyHitPoints;
        }
        #endregion

        #region Local Functions

        #endregion
    }
}
