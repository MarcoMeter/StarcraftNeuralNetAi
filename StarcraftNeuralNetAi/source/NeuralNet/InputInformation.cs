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
        private double squadHitPoints;
        private double initialSquadHp;
        private double initialEnemySquadHp;
        private double squadCount;
        private double initialSquadCount;
        private double squadDpf;
        private double enemySquadHitPoints;
        private double enemySquadCount;
        private double initialEnemySquadCount;
        private double enenmyDpf;
        // Known by CombatUnitTrainingBehavior
        private double localHitPoints;
        private double initialLocalHitPoints;
        private double localDpf;
        private bool isCompleted = false;
        #endregion

        #region Constructor
        public InputInformation(int squadHP, int initialSquadHp, int squadCount, int initialSquadCount, double squadDpf, 
                                int enemySquadHP, int initialEnemySquadHp, int enemySquadCount, int initialEnemySquadCount, double enemyDpf)
        {
            this.squadHitPoints = (double)squadHP;
            this.initialSquadHp = (double)initialSquadHp;
            this.squadCount = (double)squadCount;
            this.initialSquadCount = (double)initialSquadCount;
            this.squadDpf = squadDpf;
            this.enemySquadHitPoints = (double)enemySquadHP;
            this.initialEnemySquadHp = (double)initialEnemySquadHp;
            this.enemySquadCount = (double)enemySquadCount;
            this.initialEnemySquadCount = (double)initialEnemySquadCount;
            this.enenmyDpf = enemyDpf;
        }
        #endregion

        #region Public Functions
        /// <summary>
        /// This function normalizes all the stored information, so that it can be processed efficiently by the neural net.
        /// </summary>
        /// <returns>Normalized array of input information for the neural net.</returns>
        public double[] GetNormalizedData()
        {
            return new double[] {squadHitPoints / initialEnemySquadHp, squadCount / initialSquadCount, squadDpf / 3.75, enemySquadHitPoints / initialEnemySquadHp, enemySquadCount / initialEnemySquadCount, enenmyDpf / 3.75, localHitPoints / initialLocalHitPoints, localDpf / 0.375};
        }

        /// <summary>
        /// After the SquadSupervisor initialized the global information, the individual AiCombatUnitBehavior has to add its local information in order to complete the InputInformation.
        /// </summary>
        /// <param name="hitPoints">Remaining hit points of the combat unit.</param>
        /// <param name="weaponCooldown">Remaining cooldown till next attack.</param>
        /// <param name="velocityX">The speed of the combat unit on the x-axis.</param>
        /// <param name="velocityY">The speed of the combat unit on the y-axis.</param>
        /// <param name="isStimmed">Is the combat unit on the effect caused by the Stimpack?</param>
        public void CompleteInputData(int hitPoints, int initialHitPoints, double dpf)
        {
            this.localHitPoints = (double)hitPoints;
            this.initialLocalHitPoints = (double)initialHitPoints;
            this.localDpf = dpf;
            this.isCompleted = true;
        }

        // Getter

        /// <summary>
        /// Each input information, which is supposed to be friendly, is getting normalized first and then returned.
        /// </summary>
        /// <returns>Returns all normalized friendly values as array.</returns>
        public double[] GetFriendlyInfo()
        {
            return new double[] {squadHitPoints / initialSquadHp, squadCount / initialSquadCount, squadDpf / 3.75, localHitPoints / initialLocalHitPoints, localDpf / 0.375};
        }

        /// <summary>
        /// Each input information, which is supposed to be of the enemy, is getting normalized first and then returned.
        /// </summary>
        /// <returns>Returns all normalized enemy values as array.</returns>
        public double[] GetEnemyInfo()
        {
            return new double[] {enemySquadHitPoints / initialEnemySquadHp, enemySquadCount / initialEnemySquadCount, enenmyDpf / 3.75};
        }

        public bool IsCompleted()
        {
            return isCompleted;
        }

        public override String ToString()
        {
            return "Squad hp : " + squadHitPoints + ", Squad count : " + squadCount + ", Squad dpf : " + squadDpf + ", Enemy squad hp : " + enemySquadHitPoints + ", Enemy squad count : " + enemySquadCount +
                ", enemy squad dpf : " + enenmyDpf + ", Local hp : " + localHitPoints + ", Local dpf : " + localDpf;
        }
        #endregion

        #region Local Functions

        #endregion
    }
}
