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
        private List<CombatUnitTrainingBehavior> combatUnits = new List<CombatUnitTrainingBehavior>();
        private List<Unit> enemyCombatUnits = new List<Unit>();
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
        public void AddCombatUnit(CombatUnitTrainingBehavior unit)
        {
            combatUnits.Add(unit);
            initialSquadHp += unit.GetUnit().HitPoints;
            initialSquadCount++;
        }

        /// <summary>
        /// Add enemy combat units to the list of the SquadSupervisor.
        /// </summary>
        /// <param name="unit">enemy combat units</param>
        public void AddEnemyCombatUnit(Unit unit)
        {
            enemyCombatUnits.Add(unit);
            initialEnemySquadHp += unit.HitPoints;
            initialEnemySquadCount++;
        }

        /// <summary>
        /// This is a test AttackMove for the whole squad towards some position.
        /// </summary>
        /// <param name="targetPosition">Attack target position</param>
        public void ForceAttack(Position targetPosition, bool useStimpack)
        {
            foreach(CombatUnitTrainingBehavior unit in combatUnits)
            {
                unit.GetUnit().Attack(targetPosition, false);
                if (useStimpack)
                {
                    unit.GetUnit().UseTech(new Tech(TechType.Stim_Packs.GetHashCode()));
                }
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
        public Unit GetClosestEnemyUnit(CombatUnitTrainingBehavior requestingUnit)
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
            if (enemyCombatUnits.Count > 0)
            {
                List<Unit> sortedByHitPoints = enemyCombatUnits.OrderBy(u => u.HitPoints).ToList();
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
        public Unit GetWeakestEnemyUnit()
        {
            if (enemyCombatUnits.Count > 0)
            {
                List<Unit> sortedByHitPoints = enemyCombatUnits.OrderByDescending(u => u.HitPoints).ToList();
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

            foreach(Unit unit in enemyCombatUnits)
            {
                x += unit.Position.X;
                y += unit.Position.Y;
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
            foreach (CombatUnitTrainingBehavior combatUnit in combatUnits)
            {
                combatUnit.ExecuteStateMachine();
            }
        }

        /// <summary>
        /// OnUnitDestroy event which is triggered by the TrainingModule
        /// </summary>
        public void OnUnitDestroy(Unit destroyedUnit)
        {
            // Update enemy combat unit list
            if(destroyedUnit.Player.Id != Game.Self.Id)
            {
                enemyCombatUnits.RemoveAt(enemyCombatUnits.IndexOf(destroyedUnit));
            }
            else
            {
                // Update controlled combat units list
                for(int i = 0; i < combatUnits.Count; i++)
                {
                    if(destroyedUnit == combatUnits[i].GetUnit())
                    {
                        combatUnits.RemoveAt(i);
                    }
                }
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
            double squadDpf = 0;
            double enemyDpf = 0;

            // gather information concerning the friendly squad
            foreach (CombatUnitTrainingBehavior unit in combatUnits)
            {
                squadHP += unit.GetUnit().HitPoints;
                // determine overall dpf
                if(unit.GetUnit().IsStimmed)
                {
                    squadDpf += 0.75;
                }
                else
                {
                    squadDpf += 0.375;
                }
            }

            // gather information concerning the enemy squad
            foreach (Unit unit in enemyCombatUnits)
            {
                enemyHP += unit.HitPoints;
                // determine overall dpf
                if (unit.IsStimmed)
                {
                    enemyDpf += 0.75;
                }
                else
                {
                    enemyDpf += 0.375;
                }
            }

            InputInformation info = new InputInformation(squadHP, initialSquadHp, Game.Self.Units.Count, initialSquadCount, squadDpf,enemyHP, initialEnemySquadHp, Game.Enemy.Units.Count, initialEnemySquadCount, enemyDpf);
            
            return info;
        }
        #endregion
    }
}
