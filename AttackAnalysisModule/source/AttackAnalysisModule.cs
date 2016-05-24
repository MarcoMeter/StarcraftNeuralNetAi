using BroodWar;
using BroodWar.Api;
using BroodWar.Api.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackAnalysisModule
{
    /// <summary>
    /// Deriving from te base class AiBase, the AttackAnalysisModule is the starting point for this AI bot application.
    /// </summary>
    public class AttackAnalysisModule : AiBase
    {
        public override void OnStart()
        {
            // Config match
            Game.EnableFlag(Flag.CompleteMapInformation); // this flag makes the information about the enemy units avaible
            Game.EnableFlag(Flag.UserInput); // this flag allows the user to take action
        }
    }
}
