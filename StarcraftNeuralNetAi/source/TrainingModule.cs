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
        SquadSupervisor squadSupervisor;
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
            //Game.EnableFlag(Flag.UserInput); // this flag allows the user to take action
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
                //squadSupervisor.ForceAttack(); // 10 vs 10
                squadSupervisor.ForceAttack(Utility.ConvertTilePosition(new TilePosition(29, 27))); // send units to attack some spot between the two armies
            }

            squadSupervisor.OnFrame(); // the supervisor will trigger OnFrame on the AiCombatUnits as well.
        }

        /// <summary>
        /// OnUnitDestroy is triggered if any unit dies or gets destroyed. Keep in mind, destroyed units can still be referenced.
        /// </summary>
        /// <param name="unit"></param>
        public override void OnUnitDestroy(Unit unit)
        {
            squadSupervisor.OnUnitDestroy(unit); // pass event to the SquadSupervisor
        }
        #endregion

        #region Local Functions
        /// <summary>
        /// Creates an instance of the SquadSupervisor. Initializes the list of combat units.
        /// </summary>
        private void InitializeSquad()
        {
            squadSupervisor = SquadSupervisor.GetInstance();
            foreach(Unit unit in Game.AllUnits)
            {
                if(unit.Player == Game.Self)
                {
                    squadSupervisor.AddCombatUnit(new AiCombatUnitBehavior(unit, squadSupervisor));
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
                    squadSupervisor.AddEnemyCombatUnit(unit);
                }
            }
        }
        #endregion

        #region Public Functions

        #endregion
    }
}