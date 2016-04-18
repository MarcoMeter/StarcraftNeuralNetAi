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
        Position redPlayerStart = Utility.ConvertTilePosition(new TilePosition(28, 13));
        Position bluePlayerStart = Utility.ConvertTilePosition(new TilePosition(29, 45));
        bool isEnemySquadInitialized = false;
        #endregion

        #region Events
        /// <summary>
        /// Everything that needs to be done before the game starts, should be done inside OnStart()
        /// </summary>
        public override void OnStart()
        {
            Game.EnableFlag(Flag.CompleteMapInformation); // this flag makes the information about the enemy units avaible
            Game.SetLocalSpeed(0); // fastest game speed, maybe adding frame skipping increases game speed
            InitializeSquad(); // instantiate the SquadSupervisor and its units
        }

        /// <summary>
        /// OnFrame() is called every single frame. Avoid expensive looping code.
        /// </summary>
        public override void OnFrame()
        {
            if(!isEnemySquadInitialized && Game.AllUnits.Count > Game.Self.Units.Count)
            {
                InitializeEnemySquad();
                isEnemySquadInitialized = true;
                // new ForceAttack test
                //supervisor.ForceAttack();
            }

            supervisor.OnFrame(); // the supervisor will trigger OnFrame on the AiCombatUnits as well.
        }

        #endregion

        #region local functions
        /// <summary>
        /// Creates an instance of the SquadSupervisor. Initializes the list of combat units.
        /// </summary>
        private void InitializeSquad()
        {
            supervisor = SquadSupervisor.GetInstance();
            foreach(Unit unit in Game.AllUnits)
            {
                if(unit.Player == Game.Self)
                {
                    supervisor.AddCombatUnit(new AiCombatUnitBehavior(unit, supervisor));
                }
            }
        }

        /// <summary>
        /// Initializes the list of enemy combat units. This should be called as soon as the Game object is aware of the complete map information.
        /// </summary>
        private void InitializeEnemySquad()
        {
            foreach (Unit unit in Game.AllUnits)
            {
                if (unit.Player != Game.Self)
                {
                    supervisor.AddEnemyCombatUnit(unit);
                }
            }
        }
        #endregion

        #region public functions

        #endregion
    }
}