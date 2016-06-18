using Encog.ML.Data;
using Encog.ML.Data.Basic;
using NeuralNetTraining.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeuralNetTraining
{
    public class AttackFitness : FitnessMeasure
    {
        #region Member
        private const double m_hitWeight = 0.85;
        private const double m_killWeight = 0.5;
        private const double m_complimentMovementWeight = 1.3;
        private const double m_blameMovementWeight = 0.7;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes the AttackFitness with initial input data, the random action and the computed output.
        /// </summary>
        /// <param name="inputVevtor">The initial input information, which was used to feed the neural net.</param>
        /// <param name="randomAction">The chosen action by random.</param>
        /// <param name="computedOutput">The computed outputs by the neural net.</param>
        public AttackFitness(InputVector inputVevtor, CombatUnitState randomAction, IMLData computedOutput) : base(inputVevtor, randomAction, computedOutput)
        {
        }
        #endregion

        #region Public Functions
        /// <summary>
        /// Before an action is being executed, the FitnessMeasure is already initialized with some initial data.
        /// In order to complete the measure of how good that action was, the final state (after executing an action) has to be supplied.
        /// After that, the desired output gets computed and a data pair based on the initial input with the new desired output is getting sent to the neural net controller.
        /// </summary>
        /// <param name="finalInputInfo">Add the final InputInformation in order to compare the initial and final state of the combat which has been altered after executing an action.</param>
        /// <param name="hit">Did an attack hit its target?</param>
        /// <param name="killed">Did the hit kill its target?</param>
        public override void ComputeDataPair(InputVector finalInputVector, bool hit, bool killed)
        {
            // Step #1: Compare the unit's situation
            // Determine how the unit's situation changed
            double unitSituationRatio = finalInputVector.UnitHitPoints / m_initialInputVector.UnitHitPoints;
            double enemySituationRatio;

            // Determine how the situation towards the enemy changed
            if(hit && killed)
            {
                enemySituationRatio = m_killWeight;
            }
            else
            {
                enemySituationRatio = m_hitWeight;
            }

            // Step #2: Compute the desired adjustments for the output vector
            double desiredOutputActionAdjustment = 1 + (unitSituationRatio - enemySituationRatio);
            double desiredMovementAdjustment = 1;

            if(unitSituationRatio <= 0.5) // The closer the units gets to death, then more likely using the MoveBack action should get increased.
            {
                desiredMovementAdjustment = m_complimentMovementWeight;
            }
            else if(unitSituationRatio >= 1.15)
            {
                desiredMovementAdjustment = m_blameMovementWeight;
            }

            // Step #3: Build the new desired output vector
            double[] output = new double[m_computedOutput.Count];

            // Convert output and make adjustments to the desired and undesired outputs
            for (int i = 0; i < m_computedOutput.Count; i++)
            {
                if(i == (int)m_randomOutput)
                {
                    output[i] = ClampOutput(m_computedOutput[i] * desiredOutputActionAdjustment);
                }
                else if(i == (int)CombatUnitState.MoveBack)
                {
                    output[i] = ClampOutput(m_computedOutput[i] * desiredMovementAdjustment);
                }
                else
                {
                    output[i] = m_computedOutput[i];
                }
            }

            // Step #4: Create and forward the data pair made of the initial input vector and the adjusted output vector
            BasicMLDataPair pair = new BasicMLDataPair(new BasicMLData(m_initialInputVector.GetNormalizedData()), new BasicMLData(output));
            m_neuralNetController.AddTrainingDataPair(pair);


            double[] d = m_initialInputVector.GetNormalizedData();
            double[] f = finalInputVector.GetNormalizedData();
            PersistenceUtil.WriteLine("<<< START " + m_randomOutput.ToString() + " FITNESS >>>");
            PersistenceUtil.WriteLine("unit situation ratio: " + finalInputVector.UnitHitPoints + " / " + m_initialInputVector.UnitHitPoints + " = " + unitSituationRatio.ToString());
            PersistenceUtil.WriteLine("desired attack adjustment: 1 + (" + unitSituationRatio + " - " + enemySituationRatio + ") = " + desiredOutputActionAdjustment);
            PersistenceUtil.WriteLine("desired movement adjustment: " + desiredMovementAdjustment);
            PersistenceUtil.WriteLine("desired output: " + pair.Ideal.ToString());
            PersistenceUtil.WriteLine("         " + String.Format("|{0,5}|{1,5}|{2,5}|{3,5}|{4,5}|{5,5}|{6,5}|{7,5}|{8,5}|{9,5}|{10,5}|{11,5}|{12,5}|{13,5}|{14,5}|{15,5}|",
                                                                    "HP", "oaFHP", "oaFC", "oaEHP", "oaEC", "crFHP", "crFC", "crEHP", "crEC", "frFHP", "frFC", "frEHP", "frEC", "dist", "wHP", "wDist"));
            PersistenceUtil.WriteLine("Initial: " + String.Format("|{0,5}|{1,5}|{2,5}|{3,5}|{4,5}|{5,5}|{6,5}|{7,5}|{8,5}|{9,5}|{10,5}|{11,5}|{12,5}|{13,5}|{14,5}|{15,5}|",
                                                                    d[0], d[1], d[2], d[3], d[4], d[5], d[6], d[7], d[8], d[9], d[10], d[11], d[12], d[13], d[14], d[15]));
            PersistenceUtil.WriteLine("Final  : " + String.Format("|{0,5}|{1,5}|{2,5}|{3,5}|{4,5}|{5,5}|{6,5}|{7,5}|{8,5}|{9,5}|{10,5}|{11,5}|{12,5}|{13,5}|{14,5}|{15,5}|",
                                                                    f[0], f[1], f[2], f[3], f[4], f[5], f[6], f[7], f[8], f[9], f[10], f[11], f[12], f[13], d[14], d[15]));
            PersistenceUtil.WriteLine(" ");
        }
        #endregion

        #region Local Functions
        /// <summary>
        /// Clamps the value to be in the range 1 and 0.1
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private double ClampOutput(double value)
        {
            if(value > 1)
            {
                return 1;
            }
            else if(value < 0.01)
            {
                return 0.01;
            }
            else
            {
                return value;
            }
        }
        #endregion
    }
}
