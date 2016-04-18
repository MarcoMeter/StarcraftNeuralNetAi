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
        List<AiCombatUnitBehavior> combatUnits = new List<AiCombatUnitBehavior>();
        List<Unit> enemyCombatUnits = new List<Unit>();
        #endregion

        #region Constructor
        private SquadSupervisor()
        {

        }
        #endregion

        #region public functions
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
        /// This is just a test attack...
        /// </summary>
        public void ForceAttack()
        {
            for(int i = 0; i < 10; i++)
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

        // Setter
        #endregion

        #region SquadSupervisor logic
        public void OnFrame()
        {
            // trigger on frame on the individual ai units
            foreach(AiCombatUnitBehavior combatUnit in combatUnits)
            {
                combatUnit.OnFrame();
            }
        }
        #endregion
    }
}
