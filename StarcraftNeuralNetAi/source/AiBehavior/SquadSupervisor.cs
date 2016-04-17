using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BroodWar;
using BroodWar.Api;
using BroodWar.Api.Enum;

namespace NetworkTraining
{
    /// <summary>
    /// The SquadSupervisor is a Singleton and takes care of controlling all combat units. It pretty much assigns the goal to send units to fight.
    /// </summary>
    public class SquadSupervisor
    {
        #region Member
        private static SquadSupervisor instance;
        List<AiCombatUnitBehavior> combatUnits = new List<AiCombatUnitBehavior>();
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

        /// <summary>
        /// This is just a test attack...
        /// </summary>
        public void ForceAttack(TilePosition targetPosition)
        {
            foreach(AiCombatUnitBehavior unit in combatUnits)
            {
                Position pos = new Position(targetPosition.X, targetPosition.Y);
                unit.GetUnit().Move(pos, false);
            }
        }

        // Getter
        public int GetSquadCount()
        {
            return combatUnits.Count;
        }

        // Setter
        #endregion

        #region SquadSupervisor logic
        public void OnFrame()
        {
            foreach(AiCombatUnitBehavior combatUnit in combatUnits)
            {
                combatUnit.OnFrame();
            }
        }
        #endregion
    }
}
