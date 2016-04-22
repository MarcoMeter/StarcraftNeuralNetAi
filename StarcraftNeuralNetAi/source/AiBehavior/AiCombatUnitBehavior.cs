﻿using BroodWar;
using BroodWar.Api;
using BroodWar.Api.Enum;
using Encog.ML.Data;
using Encog.ML.Data.Basic;
using Encog.Neural.Networks;
using Encog.Persist;
using System;
using System.IO;

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
        private BasicNetwork neuralNet;
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
            // Load the artificial neural network
            this.neuralNet = (BasicNetwork)EncogDirectoryPersistence.LoadObject(new FileInfo("testNetwork" + Game.Self.Id.ToString() + ".ann"));
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
        /// The neural net will be executed several times a second, which is determined by the SquadSupervisor. Based on the decisions made by the net, the concerning state of the finite state machine will be executed.
        /// </summary>
        public void ExecuteStateMachine()
        {
            inputInfo = squadSupervisor.GetGlobalInputInformation(); // request most recent global input information

            // Complete the inputInfo with local information
            // Pure damage is not considered to be as an input, just because the damage is considered to be constant for now. Weapon cooldown is much more interesting due to stimpack and timing.
            inputInfo.CompleteInputData(unit.HitPoints, unit.GroundWeaponCooldown, unit.VelocityX, unit.VelocityY, unit.IsStimmed);
            double[] inputData = inputInfo.GetNormalizedData();

            // Use the neural net to classify the input information
            //CombatUnitState currentState = (CombatUnitState)neuralNet.Classify(new BasicMLData(inputData));
            CombatUnitState currentState = CombatUnitState.AttackClosest; // force state for testing outputs
            Game.Write(currentState.ToString());

            // state execution
            switch (currentState)
            {
                case CombatUnitState.AttackClosest:
                    AttackClosest();
                    break;
                case CombatUnitState.AttackStrongest:
                    AttackStrongest();
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
                case CombatUnitState.SquadState:
                    SquadState();
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
            unit.Attack(squadSupervisor.GetClosestEnemyUnit(this), false);
        }

        /// <summary>
        /// Attack the fastest enemy unit.
        /// </summary>
        private void AttackStrongest()
        {
            // ask SS
        }

        /// <summary>
        /// Attack the most valueable enemy unit. The value of an unit is based on its training resource costs.
        /// </summary>
        private void AttackMostValueable()
        {
            // ask SS
        }

        /// <summary>
        /// Attack the weakest enemy unit. Being weak depends on all the properties of a unit. So a valueable unit being low on health is considered to be weak compared to a cheap unit with full health.
        /// </summary>
        private void AttackWeakest()
        {
            // ask SS
        }

        /// <summary>
        /// Move towards enemy.
        /// </summary>
        private void MoveTowards()
        {
            // move towards closest enemy or center of mass of the enemy squad
        }

        /// <summary>
        /// Back up from enemy.
        /// </summary>
        private void MoveBack()
        {
            // some sort of behavior steering computation in order to back up from enemy squad or maybe just closest enemy
        }

        /// <summary>
        /// Use stimpack.
        /// </summary>
        private void UseStimpack()
        {
            unit.UseTech(new Tech(TechType.Stim_Packs.GetHashCode()));
        }

        /// <summary>
        /// Seek enemy.
        /// </summary>
        private void Seek()
        {
            // last known location?
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
