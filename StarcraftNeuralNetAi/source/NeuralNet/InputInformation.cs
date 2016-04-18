using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkTraining
{
    /// <summary>
    /// This object stores and normalizes information for the input layer of the neural network.
    /// </summary>
    public class InputInformation
    {
        #region Member
        // Known by SquadSupervisor
        private int squadHitPoints;
        private int squadCount;
        private int enemySquadHitPoints;
        private int enemySquadCount;
        // Known by AiCobatUnitBehavior
        private int localHitPoints;
        private double localDPS;
        private double localMovementSpeed;
        private bool localIsStimpackCooldown = false;
        #endregion

        #region Constructor
        public InputInformation(int squadHP, int squadCount, int enemySquadHP, int enemySquadCount)
        {
            this.squadHitPoints = squadHP;
            this.squadCount = squadCount;
            this.enemySquadHitPoints = enemySquadHP;
            this.enemySquadCount = enemySquadCount;
        }
        #endregion

        #region Public Functions
        /// <summary>
        /// This function normalizes all the stored information, so that it can be processed by the neural net.
        /// </summary>
        /// <returns>Normalized array of input information for the neural net.</returns>
        public double[] GetNormalizedData()
        {
            return null;
        }

        /// <summary>
        /// After the SquadSupervisor initialized the global information, the individual AiCombatUnitBehavior has to add its local information in order to complete the InputInformation.
        /// </summary>
        /// <param name="hitPoints"></param>
        /// <param name="dps"></param>
        /// <param name="movementSpeed"></param>
        /// <param name="stimpack"></param>
        public void CompleteInputData(int hitPoints, double dps, double movementSpeed, bool stimpack)
        {
            this.localHitPoints = hitPoints;
            this.localDPS = dps;
            this.localMovementSpeed = movementSpeed;
            this.localIsStimpackCooldown = stimpack;
        }

        // Getter
        public int GetSquadHitPoints()
        {
            return this.squadHitPoints;
        }

        public int GetSquadCount()
        {
            return this.squadCount;
        }

        public int GetEnemySquadHitPoints()
        {
            return this.enemySquadHitPoints;
        }

        public int GetEnemySquadCount()
        {
            return this.enemySquadCount;
        }
        #endregion

        #region Local Functions

        #endregion
    }
}
