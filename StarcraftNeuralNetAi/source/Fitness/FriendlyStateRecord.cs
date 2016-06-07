using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


/*

Not in use right now. Might come in handy to make the InputInformation.cs less complex.

*/



namespace NeuralNetTraining
{
    /// <summary>
    /// This class captures the state of the friendly unit's side.
    /// </summary>
    public class FriendlyStateRecord
    {
        #region Member Fields
        private int m_unitHealth;
        private int m_squadHealth;
        private int m_squadCount;
        #endregion

        #region Constructor
        /// <summary>
        /// The FriendlyStateRecord captures information of some friendly properties at the current point of time.
        /// </summary>
        /// <param name="unitHealth">Health of the individual unit.</param>
        /// <param name="squadHealth">Total hit points of all friendly units.</param>
        /// <param name="squadCount">Number of friendly units.</param>
        public FriendlyStateRecord(int unitHealth, int squadHealth, int squadCount)
        {
            this.m_unitHealth = unitHealth;
            this.m_squadHealth = squadHealth;
            this.m_squadCount = squadCount;
        }
        #endregion

        #region Member Properties
        /// <summary>
        /// Read-only health of the individual unit.
        /// </summary>
        public int UnitHealth
        {
            get
            {
                return this.m_unitHealth;
            }
        }

        /// <summary>
        /// Read-only total health of the friendly squad.
        /// </summary>
        public int SquadHealth
        {
            get
            {
                return this.m_squadHealth;
            }
        }

        /// <summary>
        /// Read-only number of friendly units.
        /// </summary>
        public int SquadCount
        {
            get
            {
                return m_squadCount;
            }
        }
        #endregion

        #region Public Functions
        /// <summary>
        /// Collects and returns the gathered friendly information as array.
        /// </summary>
        /// <returns>Returns the gathered friendly information as array.</returns>
        public double[] GetInfo()
        {
            return new double[] {m_unitHealth, m_squadHealth, m_squadCount};
        }
        #endregion
    }
}
