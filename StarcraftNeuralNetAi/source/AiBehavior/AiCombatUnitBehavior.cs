using BroodWar;
using BroodWar.Api;
using BroodWar.Api.Enum;

namespace NetworkTraining
{
    /// <summary>
    /// Each controlled combat unit should come with an AiCombatUnitBehavior in order to attach a neural net behavior to that combat unit.
    /// </summary>
    public class AiCombatUnitBehavior
    {
        #region Member
        private Unit unit;
        private SquadSupervisor squadSupervisor;
        private CombatUnitState currentState = CombatUnitState.SquadState;
        private InputInformation inputInfo;
        #endregion

        #region Constructor
        /// <summary>
        /// Create an instance of the AiCombatBehavior.
        /// </summary>
        /// <param name="unit">The individual combat unit which needs to be pared with this behavior.</param>
        /// <param name="supervisor">The SquadSupervisor which controlls the combat unit.</param>
        public AiCombatUnitBehavior(Unit unit, SquadSupervisor supervisor)
        {
            this.unit = unit;
            this.squadSupervisor = supervisor;
        }
        #endregion

        #region Public Functions
        // Getter
        public Unit GetUnit()
        {
            return this.unit;
        }

        public SquadSupervisor GetSupervisor()
        {
            return this.squadSupervisor;
        }

        public CombatUnitState GetCurrentState()
        {
            return this.currentState;
        }

        public InputInformation GetInputInformation()
        {
            return this.inputInfo;
        }

        // Setter
        public void ForceState(CombatUnitState state)
        {
            this.currentState = state;
        }
        #endregion

        #region Behavior Logic
        /// <summary>
        /// The neural net will be executed each frame. Based on the decisions made by net net, the concerning state of the finite state machine will be executed.
        /// </summary>
        public void OnFrame()
        {
            inputInfo = squadSupervisor.GetGlobalInputInformation(); // request most recent global input information

            // Complete the inputInfo with local information
            // Pure damage is not considered to be as an input, just because the damage is considered to be constant for now. Weapon cooldown is much more interesting due to stimpack and timing.
            inputInfo.CompleteInputData(unit.HitPoints, unit.GroundWeaponCooldown, unit.VelocityX, unit.VelocityY, unit.IsStimmed);

            // state decision
            // neural net stuff

            // state execution
            switch (currentState)
            {
                case CombatUnitState.SquadState:
                    SquadState();
                    break;
                case CombatUnitState.AttackClosest:
                    AttackClosest();
                    break;
                case CombatUnitState.AttackFastest:
                    AttackFastest();
                    break;
                case CombatUnitState.AttackMostValueable:
                    AttackMostValueable();
                    break;
                case CombatUnitState.AttackWeakest:
                    AttackWeakest();
                    break;
                case CombatUnitState.MoveTowards:
                    MoveTowards();
                    break;
                case CombatUnitState.MoveBack:
                    MoveBack();
                    break;
                case CombatUnitState.UseStimpack:
                    UseStimpack();
                    break;
                case CombatUnitState.Seek:
                    Seek();
                    break;
                case CombatUnitState.Retreat:
                    Retreat();
                    break;
            }

            // fitness function?
        }

        /// <summary>
        /// The initial state is idle. Basically the combat unit will wait for the neural net to take over which is commanded by the SquadSupervisor.
        /// </summary>
        private void SquadState()
        {

        }

        /// <summary>
        /// Attack the closest enemy unit.
        /// </summary>
        private void AttackClosest()
        {

        }

        /// <summary>
        /// Attack the fastest enemy unit.
        /// </summary>
        private void AttackFastest()
        {

        }

        /// <summary>
        /// Attack the most valueable enemy unit. The value of an unit is based on its training resource costs.
        /// </summary>
        private void AttackMostValueable()
        {

        }

        /// <summary>
        /// Attack the weakest enemy unit. Being weak depends on all the properties of a unit. So a valueable unit being low on health is considered to be weak compared to a cheap unit with full health.
        /// </summary>
        private void AttackWeakest()
        {

        }

        /// <summary>
        /// Move towards enemy.
        /// </summary>
        private void MoveTowards()
        {

        }

        /// <summary>
        /// Back up from enemy.
        /// </summary>
        private void MoveBack()
        {

        }

        /// <summary>
        /// Use stimpack.
        /// </summary>
        private void UseStimpack()
        {

        }

        /// <summary>
        /// Seek enemy.
        /// </summary>
        private void Seek()
        {

        }

        /// <summary>
        /// Retreat from combat.
        /// </summary>
        private void Retreat()
        {

        }
        #endregion
    }
}
