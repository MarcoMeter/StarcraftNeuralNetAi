using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeuralNetTraining
{
    /// <summary>
    /// This class captures the state of the enemy unit's side.
    /// </summary>
    public class EnemyStateRecord
    {
        #region Member Fields
        private double m_closestEnemyDistance;
        private int m_enemySquadCount;
        private int m_enemySquadHealth;
        #endregion

        #region Constructor
        /// <summary>
        /// The EnemyStateRecord captures information of some enemy properties at the current point of time.
        /// </summary>
        /// <param name="closestEnemyDistance">The distance to the closest enemy.</param>
        /// <param name="enemySquadCount">Number of enemy unit.</param>
        /// <param name="enemySquadHealth">Total health of the enemy units.</param>
        public EnemyStateRecord(double closestEnemyDistance, int enemySquadCount, int enemySquadHealth)
        {
            this.m_closestEnemyDistance = closestEnemyDistance;
            this.m_enemySquadCount = enemySquadCount;
            this.m_enemySquadHealth = enemySquadHealth;
        }
        #endregion

        #region Member Properties
        /// <summary>
        /// Read-only distance to closest enemy.
        /// </summary>
        public double ClosestEnemyDistance
        {
            get
            {
                return this.m_closestEnemyDistance;
            }
        }

        /// <summary>
        /// Read-only count of the enemy's squad.
        /// </summary>
        public int EnemySquadCount
        {
            get
            {
                return this.m_enemySquadCount;
            }
        }

        /// <summary>
        /// Read-only total health of the enemy's squad.
        /// </summary>
        public int EnemySquadHealth
        {
            get
            {
                return this.m_enemySquadHealth;
            }
        }
        #endregion

        #region Public Functions
        /// <summary>
        /// Collects and returns the gathered enemy information as array.
        /// </summary>
        /// <returns>Returns the gathered enemy information as array.</returns>
        public double[] GetInfo()
        {
            return new double[] { m_closestEnemyDistance, m_enemySquadCount, m_enemySquadHealth };
        }
        #endregion
    }
}
