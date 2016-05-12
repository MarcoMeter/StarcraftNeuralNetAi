using System;
using System.IO;
using System.Drawing;
using BroodWar.Api;
using BroodWar.Api.Enum;
using System.Collections.Generic;
using NeuralNetTraining.Utility;

namespace NeuralNetTraining
{
    public class EnemyFeedbackBehavior
    {
        #region Members
        private Unit unit;
        private int hitPoints;
        private SquadSupervisor squadSupervisor;
        private NeuralNetController neuralNetController;
        private List<CombatUnitTrainingBehavior> attackers = new List<CombatUnitTrainingBehavior>();
        private List<FitnessMeasure> attackersFitness = new List<FitnessMeasure>();
        private bool isExpectingFeedback = false;
        private bool trainingMode;
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="supervisor"></param>
        #region Constructor
        public EnemyFeedbackBehavior(Unit unit, SquadSupervisor supervisor)
        {
            this.unit = unit;
            this.hitPoints = unit.HitPoints;
            this.squadSupervisor = supervisor;
            this.neuralNetController = NeuralNetController.GetInstance();
            this.trainingMode = TrainingModule.trainingMode;
        }
        #endregion

        #region Public Functions
        public void OnAttackLaunched(FitnessMeasure fitnessMeasure, CombatUnitTrainingBehavior friendlyUnit)
        {
            attackers.Add(friendlyUnit);
            attackersFitness.Add(fitnessMeasure);
            isExpectingFeedback = true;
        }
        // Getter
        public Unit GetUnit()
        {
            return this.unit;
        }

        public SquadSupervisor GetSupervisor()
        {
            return this.squadSupervisor;
        }
        #endregion

        #region Behavior Logic
        /// <summary>
        /// OnFramne is triggered each frame by the SquadSupervisor for further logic.
        /// </summary>
        public void OnFrame()
        {
            if (isExpectingFeedback && trainingMode)
            {
                // check if damage is taken
                if (unit.HitPoints < hitPoints)
                {
                    PersistenceUtil.WriteLine("Enemy took damage, computing feedback");
                    int totalDamage = hitPoints - unit.HitPoints;
                    if (attackers.Count > 0)
                    {
                        neuralNetController.AddTrainingDataPair(attackersFitness[0].ComputeDataPair(attackers[0].GenerateInputInfo()));
                        attackers.RemoveAt(0);
                        attackersFitness.RemoveAt(0);
                    }
                    hitPoints = unit.HitPoints;
                }
                isExpectingFeedback = false;
            }
        }
        #endregion

        #region Local Functions
        #endregion
    }
}
