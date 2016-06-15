using System.Collections.Generic;
using BroodWar;
using BroodWar.Api;
using BroodWar.Api.Enum;
using System.Linq;
using NeuralNetTraining.Utility;

namespace NeuralNetTraining
{
    /// <summary>
    /// The SquadSupervisor takes care of controlling and observing all friendly and enemy combat units.
    /// Global input information is provided by the SquadSupervisor.
    /// </summary>
    public class SquadSupervisor
    {
        #region Member Fields
        // Friendly and foe behaviors
        private List<CombatUnitTrainingBehavior> m_friendlyCombatUnits = new List<CombatUnitTrainingBehavior>();
        private List<EnemyBehavior> m_enemyCombatUnits = new List<EnemyBehavior>();

        // InputInfo related objects
        private InputInformation m_globalInputInfo;
        private int m_initialSquadHp = 0;
        private int m_initialEnemySquadHp = 0;
        private int m_initialSquadCount = 0;
        private int m_initialEnemySquadCount = 0;
        #endregion

        #region Member Properties
        /// <summary>
        /// Returns the count of controlled combat units.
        /// </summary>
        public int FriendlyCount
        {
            get
            {
                return Game.Self.Units.Count;
            }
        }

        /// <summary>
        /// Returns the count of enemy combat units.
        /// </summary>
        public int EnemyCount
        {
            get
            {
                return Game.Enemy.Units.Count;
            }
        }

        /// <summary>
        /// Read-only. The SquadSupervisor provides global input information for the neural net, which is getting updated each frame.
        /// </summary>
        public InputInformation GlobalInputInfo
        {
            get
            {
                return this.m_globalInputInfo;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates an instance of the SquadSupervisor. Members are modified by public functions. Like the lists of enemy and friendly combat units.
        /// </summary>
        public SquadSupervisor()
        {

        }
        #endregion

        #region Public Functions
        /// <summary>
        /// Sums the current hit points of the friendly units.
        /// </summary>
        /// <returns>Returns the total hit points of the squad's units.</returns>
        public int GetSquadHealth()
        {
            int health = 0;

            foreach (Unit unit in Game.Self.Units)
            {
                health += unit.HitPoints;
            }

            return health;
        }

        /// <summary>
        /// Add friendly combat units to the list of the SquadSupervisor.
        /// </summary>
        /// <param name="behavior">friendly combat unit</param>
        public void AddFriendlyCombatUnit(CombatUnitTrainingBehavior behavior)
        {
            m_friendlyCombatUnits.Add(behavior);
            m_initialSquadHp += behavior.Unit.HitPoints;
            m_initialSquadCount++;
        }

        /// <summary>
        /// Add enemy combat units to the list of the SquadSupervisor.
        /// </summary>
        /// <param name="behavior">enemy combat unit</param>
        public void AddEnemyCombatUnit(EnemyBehavior behavior)
        {
            m_enemyCombatUnits.Add(behavior);
            m_initialEnemySquadHp += behavior.Unit.HitPoints;
            m_initialEnemySquadCount++;
        }

        /// <summary>
        /// Search through the friendly combat unit list to find the paired behavior to the unit.
        /// </summary>
        /// <param name="unit">The unit to search for.</param>
        /// <returns>Returns the CombatTrainingBehavior for that certain unit to seek for.</returns>
        public CombatUnitTrainingBehavior FindFriendlyUnitBehavior(Unit unit)
        {
            // find unit in behavior list
            for (int i = 0; i < m_friendlyCombatUnits.Count; i++)
            {
                if (unit == m_friendlyCombatUnits[i].Unit)
                {
                    return m_friendlyCombatUnits[i];
                }
            }

            return null; 
        }

        /// <summary>
        /// Search through the enemy combat unit list to find the paired behavior to the unit.
        /// </summary>
        /// <param name="unit">The unit to search for.</param>
        /// <returns>Returns the EnemyFeedbackBehavior for that certain unit to seek for.</returns>
        public EnemyBehavior FindEnemyUnitBehavior(Unit unit)
        {
            // find unit in behavior list
            for (int i = 0; i < m_enemyCombatUnits.Count; i++)
            {
                if (unit == m_enemyCombatUnits[i].Unit)
                {
                    return m_enemyCombatUnits[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Find the closest enemy unit.
        /// </summary>
        /// <param name="requestingUnit">Reference the requesting AiCombatUnitBehavior</param>
        /// <returns>Returns the closest enemy combat unit to the requesting controlled combat unit.</returns>
        public Unit GetClosestEnemyUnit(CombatUnitTrainingBehavior requestingUnit)
        {
            Unit closestUnit = null;
            double distance = double.MaxValue;

            // loop through all enemy units and search for the closest one
            foreach(Unit unit in Game.Enemy.Units)
            {
                double tempDistance = unit.Distance(requestingUnit.Unit);

                if(tempDistance < distance)
                {
                    closestUnit = unit;
                    distance = tempDistance;
                }
            }
            return closestUnit;
        }

        /// <summary>
        /// Find the weakest enemy unit. So far this is based on hit points.
        /// </summary>
        /// <returns>Returns the weakest enemy combat unit to the requesting controlled combat unit.</returns>
        public Unit GetWeakestEnemyUnit()
        {
            if (EnemyCount > 0)
            {
                List<Unit> sorted = Game.Enemy.Units.OrderByDescending(unit => unit.HitPoints).ToList();
                return sorted[0];
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
            
            foreach(EnemyBehavior behavior in m_enemyCombatUnits)
            {
                x += behavior.Unit.Position.X;
                y += behavior.Unit.Position.Y;
            }

            if (m_enemyCombatUnits.Count > 0)
            {
                x = x / m_enemyCombatUnits.Count;
                y = y / m_enemyCombatUnits.Count;
            }
            else
            {
                return null;
            }

            return new Position(x, y);
        }
        #endregion

        #region BWAPI Events
        /// <summary>
        /// OnFrame event which is triggered by the TrainingModule.
        /// </summary>
        public void OnFrame()
        {
            m_globalInputInfo = GatherRawInputData();

            // trigger on frame on the individual ai units
            foreach (CombatUnitTrainingBehavior friendlyCombatUnits in m_friendlyCombatUnits)
            {
                friendlyCombatUnits.OnFrame();
            }
            foreach(EnemyBehavior enemyComabtUnit in m_enemyCombatUnits)
            {
                enemyComabtUnit.OnFrame();
            }
        }

        /// <summary>
        /// OnUnitDestroy event which is triggered by the TrainingModule. It sets the IsAlive property of the particular unit behavior to false.
        /// </summary>
        public void OnUnitDestroy(Unit destroyedUnit)
        {
            // Update friendly or enemy combat unit list
            if (destroyedUnit.Player.Id != Game.Self.Id)
            {
                FindEnemyUnitBehavior(destroyedUnit).IsAlive = false;
            }
            else
            {
                FindFriendlyUnitBehavior(destroyedUnit).IsAlive = false;
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
            foreach (EnemyBehavior behavior in m_enemyCombatUnits)
            {
                enemyHP += behavior.Unit.HitPoints;
            }

            InputInformation info = new InputInformation(enemyHP, m_initialEnemySquadHp, EnemyCount, m_initialEnemySquadCount, GetSquadHealth(), m_initialSquadHp, FriendlyCount, m_initialSquadCount);
            
            return info;
        }
        #endregion
    }
}
