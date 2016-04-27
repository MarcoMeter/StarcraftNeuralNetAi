using BroodWar.Api;
using System;

namespace NetworkTraining
{
    public class Utility
    {
        #region Member
        public static Random randomNumberGenerator = new Random();
        #endregion

        /// <summary>
        /// Converts a TilePosition to a Position. Basically the TilePosition gets multiplied by 32.
        /// </summary>
        /// <param name="tilePosition"></param>
        /// <returns>Converted Position</returns>
        public static Position ConvertTilePosition(TilePosition tilePosition)
        {
            return new Position(tilePosition.X * 32, tilePosition.Y * 32);
        }
    }
}
