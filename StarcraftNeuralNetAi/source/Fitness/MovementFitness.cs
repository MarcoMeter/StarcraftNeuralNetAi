using Encog.ML.Data;
using Encog.ML.Data.Basic;
using NeuralNetTraining.Utility;
using System;

namespace NeuralNetTraining
{
    /// <summary>
    /// The MovementFitness measures the fitness of movement actions. Be aware, this fitness measure doesn't work reliably, yet.
    /// </summary>
    public class MovementFitness : FitnessMeasure
    {
        #region Constructor
        /// <summary>
        /// Initializes the MovementFitness with initial input data, the random action and the computed output.
        /// </summary>
        /// <param name="inputVector">The initial input information, which was used to feed the neural net.</param>
        /// <param name="randomAction">The chosen action by random.</param>
        /// <param name="computedOutput">The computed outputs by the neural net.</param>
        public MovementFitness(InputVector inputVector, CombatUnitState randomAction, IMLData computedOutput) : base(inputVector, randomAction, computedOutput)
        {
        }
        #endregion

        #region Public Functions
        /// <summary>
        /// Before an action is being executed, the FitnessMeasure is already initialized with some initial data.
        /// In order to complete the measure of how good that action was, the final state (after executing an action) has to be supplied.
        /// After that, the desired output gets computed and a data pair based on the initial input with the new desired output is sent to the neural net controller.
        /// </summary>
        /// <param name="finalInputInfo">Add the final InputInformation in order to compare the initial and final state of the combat which has been altered after executing an action.</param>
        /// <param name="hit">In the case of movement enter false</param>
        /// <param name="killed">In the case of movement enter false</param>
        public override void ComputeDataPair(InputVector finalInputVector, bool hit, bool killed)
        {
            // Step #1: Compare the unit's situation
            // Determine how the unit's situation changed
            double distanceRatio = m_initialInputVector.ClosestEnemyDistance / finalInputVector.UnitHitPoints;
            double closeSituation = ComputeRatio(new double[] {m_initialInputVector.CloseRangeFriendlyHitPoints, m_initialInputVector.CloseRangeFriendlyCount }, new double[] {finalInputVector.CloseRangeFriendlyHitPoints, finalInputVector.CloseRangeFriendlyCount})
                                    - ComputeRatio(new double[] {m_initialInputVector.CloseRangeEnemyHitPoints, m_initialInputVector.CloseRangeEnemyCount }, new double[] {finalInputVector.CloseRangeEnemyHitPoints, finalInputVector.CloseRangeEnemyCount });
            double farSituation = ComputeRatio(new double[] {m_initialInputVector.FarRangeFriendlyHitPoints, m_initialInputVector.FarRangeFriendlyCount }, new double[] {finalInputVector.FarRangeFriendlyHitPoints, finalInputVector.FarRangeFriendlyCount })
                                    - ComputeRatio(new double[] {m_initialInputVector.FarRangeEnemyHitPoints, m_initialInputVector.FarRangeEnemyCount }, new double[] {finalInputVector.FarRangeEnemyHitPoints, finalInputVector.FarRangeEnemyCount });

            // Step #2: Compute the desired adjustments for the output vector
            // weight close and far situation
            closeSituation *= 1.5;
            farSituation *= 0.5;
            // compute adjustment
            double desiredMovementAdjustment = 1 + ((closeSituation + farSituation) / 2);


            // Step #3: Build the new desired output vector
            double[] output = new double[m_computedOutput.Count];

            // Convert output and make adjustments to the desired and undesired outputs
            for (int i = 0; i < m_computedOutput.Count; i++)
            {
                if(i == (int)m_randomOutput)
                {
                    output[i] = m_computedOutput[i];
                }
                else
                {
                    output[i] = m_computedOutput[i];
                }
            }

            // Step #4: Create and forward the data pair made of the initial input vector and the adjusted output vector
            BasicMLDataPair pair = new BasicMLDataPair(new BasicMLData(m_initialInputVector.GetNormalizedData()), new BasicMLData(output));
            m_neuralNetController.AddTrainingDataPair(pair);

            // Logs
            //double[] d = m_initialInputVector.GetNormalizedData();
            //double[] f = finalInputVector.GetNormalizedData();
            //PersistenceUtil.WriteLine("<<< START " + m_randomOutput.ToString() + " FITNESS >>>");
            //PersistenceUtil.WriteLine("distanceRatio: " + distanceRatio);
            //PersistenceUtil.WriteLine("closeSituation: " + closeSituation);
            //PersistenceUtil.WriteLine("farSituation: " + farSituation);
            //PersistenceUtil.WriteLine("desiredMovementAdjustment: 1 + ((" + closeSituation + " + " + farSituation + ") / 2 = " + desiredMovementAdjustment);
            ////PersistenceUtil.WriteLine("desired output: " + pair.Ideal.ToString());
            //PersistenceUtil.WriteLine("         " + String.Format("|{0,5}|{1,5}|{2,5}|{3,5}|{4,5}|{5,5}|{6,5}|{7,5}|{8,5}|{9,5}|{10,5}|{11,5}|{12,5}|{13,5}|{14,5}|{15,5}|",
            //                                                        "HP", "oaFHP", "oaFC", "oaEHP", "oaEC", "crFHP", "crFC", "crEHP", "crEC", "frFHP", "frFC", "frEHP", "frEC", "dist", "wHP", "wDist"));
            //PersistenceUtil.WriteLine("Initial: " + String.Format("|{0,5}|{1,5}|{2,5}|{3,5}|{4,5}|{5,5}|{6,5}|{7,5}|{8,5}|{9,5}|{10,5}|{11,5}|{12,5}|{13,5}|{14,5}|{15,5}|",
            //                                                        d[0], d[1], d[2], d[3], d[4], d[5], d[6], d[7], d[8], d[9], d[10], d[11], d[12], d[13], d[14], d[15]));
            //PersistenceUtil.WriteLine("Final  : " + String.Format("|{0,5}|{1,5}|{2,5}|{3,5}|{4,5}|{5,5}|{6,5}|{7,5}|{8,5}|{9,5}|{10,5}|{11,5}|{12,5}|{13,5}|{14,5}|{15,5}|",
            //                                                        f[0], f[1], f[2], f[3], f[4], f[5], f[6], f[7], f[8], f[9], f[10], f[11], f[12], f[13], d[14], d[15]));
            //PersistenceUtil.WriteLine(" ");
        }
        #endregion

        #region Local Functions
        /// <summary>
        /// Computes a ratio between two arrays of the same length featuring initial and final input information. Cases concering arithmetic exceptions are treated individually.
        /// </summary>
        /// <param name="initial">Initial input information</param>
        /// <param name="final">Final input information</param>
        /// <returns>Returns the computed ratio between both arrays.</returns>
        private double ComputeRatio(double[] initial, double[] final)
        {
            double ratio = 0;
            int numData = 0;
            for(int i = 0; i < initial.Length; i++)
            {
                // It's important to take care of the cases if one of the incoming values is 0, not just because of the fact that division by 0 is impossible (arithmetic exception).
                if (initial[i] == 0 && final[i] == 0)
                {
                    ratio++;
                }
                else if(initial[i] == 0 && final[i] != 0)
                {
                    ratio += 2;
                }
                else if(initial[i] != 0 && final[i] == 0)
                {
                    ratio += 0;
                }
                else
                {
                    ratio += (final[i] / initial[i]);
                }
                numData++;
            }
            ratio /= numData;
            return ratio;
        }
        #endregion
    }
}
