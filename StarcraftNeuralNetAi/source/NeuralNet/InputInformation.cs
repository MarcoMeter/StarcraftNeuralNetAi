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
        private int localWeaponCooldown;
        private double velocityX;
        private double velocityY;
        private bool isStimmed = false;
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
        /// <param name="hitPoints">Remaining hit points of the combat unit.</param>
        /// <param name="weaponCooldown">Remaining cooldown till next attack.</param>
        /// <param name="velocityX">The speed of the combat unit on the x-axis.</param>
        /// <param name="velocityY">The speed of the combat unit on the y-axis.</param>
        /// <param name="isStimmed">Is the combat unit on the effect caused by the Stimpack?</param>
        public void CompleteInputData(int hitPoints, int weaponCooldown, double velocityX, double velocityY, bool isStimmed)
        {
            this.localHitPoints = hitPoints;
            this.localWeaponCooldown = weaponCooldown;
            this.velocityX = velocityX;
            this.velocityY = velocityY;
            this.isStimmed = isStimmed;
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

        public override String ToString()
        {
            return "Squad hp : " + squadHitPoints + ", Squad count : " + squadCount + ", Enemy squad hp : " + enemySquadHitPoints + ", Enemy squad count : " + enemySquadCount +
                ", Local hp : " + localHitPoints + ", Local cd : " + localWeaponCooldown + ", Local velocity : (" + velocityX + "/" + velocityY + "), Local isStimmed : " + isStimmed;
        }
        #endregion

        #region Local Functions

        #endregion
    }
}
