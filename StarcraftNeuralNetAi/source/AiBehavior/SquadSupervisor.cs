using System.Collections.Generic;
using BroodWar;
using BroodWar.Api;
using BroodWar.Api.Enum;
using System.Linq;

namespace NetworkTraining
{
    /// <summary>
    /// The SquadSupervisor is a Singleton and takes care of controlling all combat units. It pretty much assigns the goal to send units to combat.
    /// </summary>
    public class SquadSupervisor
    {
        #region Member
        private List<AiCombatUnitBehavior> combatUnits = new List<AiCombatUnitBehavior>();
        private List<Unit> enemyCombatUnits = new List<Unit>();
        private InputInformation globalInputInfo;
        #endregion

        #region Constructor
        public SquadSupervisor()
        {

        }
        #endregion

        #region Public Functions
        /// <summary>
        /// Add owned combat units to the list of the SquadSupervisor.
        /// </summary>
        /// <param name="unit">friendly combat units</param>
        public void AddCombatUnit(AiCombatUnitBehavior unit)
        {
            combatUnits.Add(unit);
        }

        /// <summary>
        /// Add not owned combat units to the list of the SquadSupervisor.
        /// </summary>
        /// <param name="unit">enemy combat units</param>
        public void AddEnemyCombatUnit(Unit unit)
        {
            enemyCombatUnits.Add(unit);
        }

        /// <summary>
        /// This is just a test attack towards some position.
        /// </summary>
        /// <param name="targetPosition">Attack target position</param>
        public void ForceAttack(Position targetPosition)
        {
            foreach(AiCombatUnitBehavior unit in combatUnits)
            {
                unit.GetUnit().Attack(targetPosition, false);
                // use stim pack
                //unit.GetUnit().UseTech(new Tech(TechType.Stim_Packs.GetHashCode()));
            }
        }

        /// <summary>
        /// This is just a test attack. It assigns one friendly combat unit to attack one enemy combat unit.
        /// </summary>
        public void ForceAttack()
        {
            for(int i = 0; i < Game.AllUnits.Count/2; i++)
            {
                combatUnits[i].GetUnit().Attack(enemyCombatUnits[i], false);
            }
        }

        // Getter
        /// <summary>
        /// Returns the count of all controlled combat units.
        /// </summary>
        /// <returns>Returns the count of controlled combat units.</returns>
        public int GetSquadCount()
        {
            return combatUnits.Count;
        }

        /// <summary>
        /// Returns the count of all enemy combat units.
        /// </summary>
        /// <returns>Returns the count of enemy combat units.</returns>
        public int GetEnemySquadCount()
        {
            return enemyCombatUnits.Count;
        }

        /// <summary>
        /// The SquadSupervisor provides global input information for the neural net, which is getting updated each frame.
        /// </summary>
        /// <returns>Global part of the input information for the neural net.</returns>
        public InputInformation GetGlobalInputInformation()
        {
            return this.globalInputInfo;
        }

        /// <summary>
        /// Find the closest enemy unit.
        /// </summary>
        /// <param name="requestingUnit">Reference the requesting AiCombatUnitBehavior</param>
        /// <returns>Returns the closest enemy combat unit to the requesting controlled combat unit.</returns>
        public Unit GetClosestEnemyUnit(AiCombatUnitBehavior requestingUnit)
        {
            Unit closestUnit = null;
            double distance = 100000;

            // loop through all enemy units and search for the closest one
            foreach(Unit unit in enemyCombatUnits)
            {
                double tempDistance = unit.Distance(requestingUnit.GetUnit());

                if(tempDistance < distance)
                {
                    closestUnit = unit;
                    distance = tempDistance;
                }
            }
            return closestUnit;
        }

        /// <summary>
        /// Find the strongest enemy unit. For now this is based on the unit's hit points.
        /// </summary>
        /// <returns>Returns the strongest enemy combat unit to the requesting controlled combat unit.</returns>
        public Unit GetStrongestEnemyUnit()
        {
            List<Unit> sortedByHitPoints = enemyCombatUnits.OrderBy(u => u.HitPoints).ToList();
            return sortedByHitPoints[0];
        }

        /// <summary>
        /// Find the weakest enemy unit. So far this is based on hit points.
        /// </summary>
        /// <returns>Returns the weakest enemy combat unit to the requesting controlled combat unit.</returns>
        public Unit GetWeakestEnemyUnit()
        {
            List<Unit> sortedByHitPoints = enemyCombatUnits.OrderByDescending(u => u.HitPoints).ToList();
            return sortedByHitPoints[0];
        }

        /// <summary>
        /// Computes the mean of every single location of each enemy combat unit.
        /// </summary>
        /// <returns>Returns the mean of all enemy units' position.</returns>
        public Position GetEnemySquadCenter()
        {
            int x = 0;
            int y = 0;

            foreach(Unit unit in enemyCombatUnits)
            {
                x += unit.Position.X;
                y += unit.Position.Y;
            }

            x = x / enemyCombatUnits.Count;
            y = y / enemyCombatUnits.Count;

            return new Position(x, y);
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

            if (Game.FrameCount % 3 == 0) // A combat unit can't be commanded each frame properly.
            {
                // trigger on frame on the individual ai units
                foreach (AiCombatUnitBehavior combatUnit in combatUnits)
                {
                    combatUnit.ExecuteStateMachine();
                }
            }
        }

        /// <summary>
        /// OnUnitDestroy event which is passed down by the SquadSupervisor
        /// </summary>
        public void OnUnitDestroy(Unit destroyedUnit)
        {
            // Update enemy combat unit list
            if(destroyedUnit.Player.Id != Game.Self.Id)
            {
                enemyCombatUnits.RemoveAt(enemyCombatUnits.IndexOf(destroyedUnit));
            }
        }
        #endregion

        #region Local Functions
        /// <summary>
        /// This function collects the global input information for the neural net.
        /// </summary>
        /// <returns>An incomplete InputInformation, which only consists of global input information, is returned.</returns>
        private InputInformation GatherRawInputData()
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

            InputInformation info = new InputInformation(squadHP, Game.Self.Units.Count, enemyHP, Game.Enemy.Units.Count);
            
            return info;
        }
        #endregion
    }
}
