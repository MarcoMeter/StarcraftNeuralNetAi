using System.Collections.Generic;
using BroodWar;
using BroodWar.Api;
using BroodWar.Api.Enum;
using System.Linq;
using NeuralNetTraining.Utility;

namespace NeuralNetTraining
{
    /// <summary>
    /// The SquadSupervisor takes care of controlling all combat units. It pretty much assigns the goal to send units to combat.
    /// </summary>
    public class SquadSupervisor
    {
        #region Member
        private List<CombatUnitTrainingBehavior> friendlyCombatUnits = new List<CombatUnitTrainingBehavior>();
        private List<EnemyFeedbackBehavior> enemyCombatUnits = new List<EnemyFeedbackBehavior>();
        private InputInformation globalInputInfo;
        private int initialSquadHp = 0;
        private int initialEnemySquadHp = 0;
        private int initialSquadCount = 0;
        private int initialEnemySquadCount = 0;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates an instance of the SquadSupervisor. Members are modified by public functions.
        /// </summary>
        public SquadSupervisor()
        {

        }
        #endregion

        #region Public Functions
        /// <summary>
        /// Add controlled combat units to the list of the SquadSupervisor.
        /// </summary>
        /// <param name="unit">friendly combat units</param>
        public void AddFriendlyCombatUnit(CombatUnitTrainingBehavior unit)
        {
            friendlyCombatUnits.Add(unit);
            initialSquadHp += unit.GetUnit().HitPoints;
            initialSquadCount++;
        }

        /// <summary>
        /// Add enemy combat units to the list of the SquadSupervisor.
        /// </summary>
        /// <param name="unit">enemy combat units</param>
        public void AddEnemyCombatUnit(EnemyFeedbackBehavior unit)
        {
            enemyCombatUnits.Add(unit);
            initialEnemySquadHp += unit.GetUnit().HitPoints;
            initialEnemySquadCount++;
        }

        /// <summary>
        /// Search through the friendly combat unit list to find the paired behavior to the unit.
        /// </summary>
        /// <param name="unit">The unit to search for.</param>
        /// <returns>Returns the CombatTrainingBehavior for that certain unit to seek for.</returns>
        public CombatUnitTrainingBehavior FindFriendlyUnitBehavior(Unit unit)
        {
            // find unit in behavior list
            for (int i = 0; i < friendlyCombatUnits.Count; i++)
            {
                if (unit == friendlyCombatUnits[i].GetUnit())
                {
                    return friendlyCombatUnits[i];
                }
            }

            return null; 
        }

        /// <summary>
        /// Search through the enemy combat unit list to find the paired behavior to the unit.
        /// </summary>
        /// <param name="unit">The unit to search for.</param>
        /// <returns>Returns the EnemyFeedbackBehavior for that certain unit to seek for.</returns>
        public EnemyFeedbackBehavior FindEnemyUnitBehavior(Unit unit)
        {
            // find unit in behavior list
            for (int i = 0; i < enemyCombatUnits.Count; i++)
            {
                if (unit == enemyCombatUnits[i].GetUnit())
                {
                    return enemyCombatUnits[i];
                }
            }

            return null;
        }


        // Getter
        /// <summary>
        /// Returns the count of all controlled combat units.
        /// </summary>
        /// <returns>Returns the count of controlled combat units.</returns>
        public int GetFriendlyCount()
        {
            return friendlyCombatUnits.Count;
        }

        /// <summary>
        /// Returns the count of all enemy combat units.
        /// </summary>
        /// <returns>Returns the count of enemy combat units.</returns>
        public int GetEnemyCount()
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
        public Unit GetClosestEnemyUnit(CombatUnitTrainingBehavior requestingUnit)
        {
            Unit closestUnit = null;
            double distance = 100000;

            // loop through all enemy units and search for the closest one
            foreach(EnemyFeedbackBehavior unit in enemyCombatUnits)
            {
                double tempDistance = unit.GetUnit().Distance(requestingUnit.GetUnit());

                if(tempDistance < distance)
                {
                    closestUnit = unit.GetUnit();
                    distance = tempDistance;
                }
            }
            return closestUnit;
        }

        /// <summary>
        /// Find the strongest enemy unit. For now this is based on the unit's hit points.
        /// </summary>
        /// <returns>Returns the strongest enemy combat unit to the requesting controlled combat unit.</returns>
        public EnemyFeedbackBehavior GetStrongestEnemyUnit()
        {
            if (enemyCombatUnits.Count > 0)
            {
                List<EnemyFeedbackBehavior> sortedByHitPoints = enemyCombatUnits.OrderBy(u => u.GetUnit().HitPoints).ToList();
                return sortedByHitPoints[0];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Find the weakest enemy unit. So far this is based on hit points.
        /// </summary>
        /// <returns>Returns the weakest enemy combat unit to the requesting controlled combat unit.</returns>
        public EnemyFeedbackBehavior GetWeakestEnemyUnit()
        {
            if (enemyCombatUnits.Count > 0)
            {
                List<EnemyFeedbackBehavior> sortedByHitPoints = enemyCombatUnits.OrderByDescending(u => u.GetUnit().HitPoints).ToList();
                return sortedByHitPoints[0];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Computes the mean of every single location of each enemy combat unit.
        /// </summary>
        /// <returns>Returns the mean of all enemy units' position.</returns>
        public Position GetEnemySquadCenter()
        {
            int x = 0;
            int y = 0;

            foreach(EnemyFeedbackBehavior unit in enemyCombatUnits)
            {
                x += unit.GetUnit().Position.X;
                y += unit.GetUnit().Position.Y;
            }

            if (enemyCombatUnits.Count > 0)
            {
                x = x / enemyCombatUnits.Count;
                y = y / enemyCombatUnits.Count;
            }
            else
            {
                return null;
            }

            return new Position(x, y);
        }
        // Setter
        #endregion

        #region Events
        /// <summary>
        /// OnFrame event which is triggered by the TrainingModule.
        /// </summary>
        public void OnFrame()
        {
            globalInputInfo = GatherRawInputData();

            // trigger on frame on the individual ai units
            foreach (CombatUnitTrainingBehavior friendlyCombatUnits in friendlyCombatUnits)
            {
                friendlyCombatUnits.OnFrame();
            }
            foreach(EnemyFeedbackBehavior enemyComabtUnit in enemyCombatUnits)
            {
                enemyComabtUnit.OnFrame();
            }
        }

        /// <summary>
        /// OnUnitDestroy event which is triggered by the TrainingModule
        /// </summary>
        public void OnUnitDestroy(Unit destroyedUnit)
        {
            // Update friendly or enemy combat unit list
            if (destroyedUnit.Player.Id != Game.Self.Id)
            {
                enemyCombatUnits.Remove(FindEnemyUnitBehavior(destroyedUnit));
            }
            else
            {
                friendlyCombatUnits.Remove(FindFriendlyUnitBehavior(destroyedUnit));
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
            int enemyHP = 0;

            // gather information concerning the enemy squad
            foreach (EnemyFeedbackBehavior unit in enemyCombatUnits)
            {
                enemyHP += unit.GetUnit().HitPoints;
            }

            InputInformation info = new InputInformation(enemyHP, initialEnemySquadHp);
            
            return info;
        }
        #endregion
    }
}
