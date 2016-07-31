using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeuralNetTraining
{
    /// <summary>
    /// The GlobalInputInfo stores information of the global game state known by the SquadSupervisor.
    /// </summary>
    public struct GlobalInputInfo
    {
        public double overAllFriendlyHitPoints;
        public double overAllFriendlyCount;
        public double overAllEnemyHitPoints;
        public double overAllEnemyCount;
        public double overAllInitialFriendlyHitPoints;
        public double overAllInitialFriendlyCount;
        public double overAllInitialEnemyHitPoints;
        public double overAllInitialEnemyCount;
        public double weakestEnemyHitPoints;
    }
}
