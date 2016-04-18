using System.Collections.Generic;
using BroodWar;
using BroodWar.Api;
using BroodWar.Api.Enum;

namespace NetworkTraining
{
    /// <summary>
    /// The SquadSupervisor is a Singleton and takes care of controlling all combat units. It pretty much assigns the goal to send units to combat.
    /// </summary>
    public class SquadSupervisor
    {
        #region Member
        private static SquadSupervisor instance;
        private List<AiCombatUnitBehavior> combatUnits = new List<AiCombatUnitBehavior>();
        private List<Unit> enemyCombatUnits = new List<Unit>();
        private InputInformation globalInputInfo;
        #endregion

        #region Constructor
        private SquadSupervisor()
        {

        }
        #endregion

        #region Public Functions
        /// <summary>
        /// If there is no instance, the supervisor will be instantiated.
        /// </summary>
        /// <returns>The only instance of the SquadSupervisor will be returned</returns>
        public static SquadSupervisor GetInstance()
        {
            if(instance == null)
            {
                instance = new SquadSupervisor();
            }
            return instance;
        }

        public void AddCombatUnit(AiCombatUnitBehavior unit)
        {
            combatUnits.Add(unit);
        }

        public void AddEnemyCombatUnit(Unit unit)
        {
            enemyCombatUnits.Add(unit);
        }

        /// <summary>
        /// This is just a test attack...
        /// </summary>
        public void ForceAttack(Position targetPosition)
        {
            foreach(AiCombatUnitBehavior unit in combatUnits)
            {
                unit.GetUnit().Attack(targetPosition, false);
            }
        }

        /// <summary>
        /// This is just a test attack... this is hard coded for attacking 10 vs 10
        /// </summary>
        public void ForceAttack()
        {
            for(int i = 0; i < Game.AllUnits.Count/2; i++)
            {
                combatUnits[i].GetUnit().Attack(enemyCombatUnits[i], false);
            }
        }

        // Getter
        public int GetSquadCount()
        {
            return combatUnits.Count;
        }

        public int GetEnemySquadCount()
        {
            return enemyCombatUnits.Count;
        }

        public InputInformation GetGlobalInputInformation()
        {
            return this.globalInputInfo;
        }

        // Setter
        #endregion

        #region SquadSupervisor logic
        /// <summary>
        /// OnFrame event which is passed down by the SquadSupervisor
        /// </summary>
        public void OnFrame()
        {
            globalInputInfo = GatherRawInputData();

            // trigger on frame on the individual ai units
            foreach(AiCombatUnitBehavior combatUnit in combatUnits)
            {
                combatUnit.OnFrame();
            }
        }

        /// <summary>
        /// OnUnitDestroy event which is passed down by the SquadSupervisor
        /// </summary>
        public void OnUnitDestroy(Unit unit)
        {

        }
        #endregion

        #region Local Functions
        /// <summary>
        /// This function collects the global input information for the neural net.
        /// </summary>
        /// <returns></returns>
        InputInformation GatherRawInputData()
        {
            int squadHP = 0;
            int enemyHP = 0;

            // add hit points of all squad units
            foreach(AiCombatUnitBehavior unit in combatUnits)
            {
                squadHP += unit.GetUnit().HitPoints;
            }

            // add hit points of all enemy units
            foreach(Unit unit in enemyCombatUnits)
            {
                enemyHP += unit.HitPoints;
            }

            InputInformation info = new InputInformation(squadHP, combatUnits.Count, enemyHP, enemyCombatUnits.Count);

            return info;
        }
        #endregion
    }
}
