using BroodWar.Api;
using System;

namespace NeuralNetTraining.Utility
{
    /// <summary>
    /// The GeneralUtil provides some useful functions for this application. Like converting a TilePosition to a Position object.
    /// </summary>
    public class GeneralUtil
    {
        #region Member Fields
        private static Random m_randomNumberGenerator = new Random(); // to avoid mal-functioning random numbers, there should be only one Random object for one specific purpose.
        private const int m_tileConversionFactor = 32;
        #endregion

        #region Member Properties
        /// <summary>
        /// Read-only Random object for the purpose of choosing random actions.
        /// </summary>
        public static Random RandomNumberGenerator
        {
            get
            {
                return m_randomNumberGenerator;
            }
        }
        #endregion

        /// <summary>
        /// Converts a TilePosition to a Position. Basically the TilePosition gets multiplied by 32.
        /// </summary>
        /// <param name="tilePosition"></param>
        /// <returns>Converted Position</returns>
        public static Position ConvertTilePosition(TilePosition tilePosition)
        {
            return new Position(tilePosition.X * m_tileConversionFactor, tilePosition.Y * m_tileConversionFactor);
        }
    }
}
