using System.ComponentModel;
using System.Drawing;
using System.Linq;
using BroodWar;
using BroodWar.Api;
using BroodWar.Api.Enum;

namespace NetworkTraining
{
    /// <summary>
    /// Entry point of the injected AI. AiBase implements several event listeners.
    /// The TrainingModule supervises the training environment.
    /// </summary>
    public class TrainingModule : AiBase
    {
        #region Member
        SquadSupervisor supervisor;
        TilePosition redPlayerStart = new TilePosition(28,13);
        TilePosition bluePlayerStart = new TilePosition(29, 45);
        #endregion

        #region Events
        public override void OnStart()
        {
            Game.Write("Training module online");
            Game.SetLocalSpeed(0);
            Game.EnableFlag(Flag.CompleteMapInformation);
            Game.Write("Match config done");
            InitializeSquad();
            Game.Write("Squad initialized");
        }

        public override void OnFrame()
        {
            supervisor.OnFrame(); // the supervisor will trigger OnFrame on the AiCombatUnits as well.

            if (Game.Self.Color == Color.Red)
            {
                supervisor.ForceAttack(bluePlayerStart);
            }
            else
            {
                supervisor.ForceAttack(redPlayerStart);
            }
        }
        #endregion

        #region local functions
        /// <summary>
        /// 
        /// </summary>
        private void InitializeSquad()
        {
            supervisor = SquadSupervisor.GetInstance();
            foreach(Unit unit in Game.Self.Units)
            {
                if(unit.UnitType.Type == BroodWar.Api.Enum.UnitType.Terran_Marine)
                {
                    supervisor.AddCombatUnit(new AiCombatUnitBehavior(unit, supervisor));
                }
            }
            Game.Write("Army count: " + supervisor.GetSquadCount());
        }
        #endregion

        #region public functions

        #endregion
    }
}