using Encog.ML.Data;
using Encog.ML.Data.Basic;
using NeuralNetTraining.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeuralNetTraining
{
    /// <summary>
    /// The fitness measure is based on a few state recordings. It compares the losses of the friendly units with enemy ones on different points in time. The result is used to favor or disfavor the executed action's output.
    /// The utility of attack actions get evaluated this way.
    /// </summary>
    public class AttackFitnessMeasure
    {
        #region Member Fields
        private InputInformation m_inputInfo;
        private CombatUnitState m_randomOutput;
        private IMLData m_computedOutput;
        private NeuralNetController m_neuralNetController;
        #endregion

        #region Constructor
        /// <summary>
        /// Based on the initial state of the combat, the random action to be executed and the computed outputs by the net, the FitnessMeasure is getting initialized.
        /// The actual fitness is measured after supplying the final state of the combat.
        /// </summary>
        /// <param name="inputInfo">The initial input information, which was used to feed the neural net.</param>
        /// <param name="randomAction">The chosen action by random.</param>
        /// <param name="computedOutput">The computed outputs by the neural net.</param>
        public AttackFitnessMeasure(InputInformation inputInfo, CombatUnitState randomAction, IMLData computedOutput)
        {
            this.m_inputInfo = inputInfo;
            this.m_randomOutput = randomAction;
            this.m_computedOutput = computedOutput;
            this.m_neuralNetController = NeuralNetController.Instance;
        }
        #endregion

        #region Public Functions
        /// <summary>
        /// Before an action is being executed, the FitnessMeasure is already initialized with some initial data.
        /// In order to complete the measure of how good that action was, the final state (after executing an action) has to be supplied.
        /// After that, the desired output gets computed and a data pair based on the initial input with the new desired output is getting returned.
        /// </summary>
        /// <param name="final">Add the final InputInformation in order to compare the initial and final state of the combat which has been altered after executing an action.</param>
        /// <returns>Returns a data pair based on the initial input and the computed desired output.</returns>
        public void ComputeDataPair(InputInformation final)
        {
            // assign the state recordings
            double[] friendlyInitial = m_inputInfo.GetFriendlyInfo();
            double[] enemyInitial = m_inputInfo.GetEnemyInfo();
            double[] friendlyFinal = final.GetFriendlyInfo();
            double[] enemyFinal = final.GetEnemyInfo();
            double friendRatio = 0;
            double enemyRatio = 0;
            int ratioDataCount = 0;

            //PersistenceUtil.WriteLine("<<< START FITNESS >>>");
            //PersistenceUtil.WriteLine("Random Action : " + (int)m_randomOutput + " " + m_randomOutput.ToString());
            //PersistenceUtil.WriteLine("Initial Friend: " + String.Join(",", friendlyInitial.Select(p => p.ToString()).ToArray()));
            //PersistenceUtil.WriteLine("Initial Enemy : " + String.Join(",", enemyInitial.Select(p => p.ToString()).ToArray()));
            //PersistenceUtil.WriteLine("Final   Friend: " + String.Join(",", friendlyFinal.Select(p => p.ToString()).ToArray()));
            //PersistenceUtil.WriteLine("Final   Enemy : " + String.Join(",", enemyFinal.Select(p => p.ToString()).ToArray()));

            // compute friendly ratio
            for (int i = 0; i < friendlyFinal.Length; i++)
            {
                if(friendlyInitial[i] > 0) // avoid division by 0
                {
                    friendRatio += (friendlyFinal[i] / friendlyInitial[i]);
                    ratioDataCount++;
                }
            }

            if(ratioDataCount > 0) // avoid division by 0
            {
                friendRatio /= ratioDataCount;
            }

            //PersistenceUtil.WriteLine("Ratio   Friend: " + friendRatio);
            ratioDataCount = 0;
            
            // compute enemy ratio
            for(int i = 0; i < enemyFinal.Length; i++)
            {
                if(enemyInitial[i] > 0) // avoid division by 0
                {
                    enemyRatio += (enemyFinal[i] / enemyInitial[i]);
                    ratioDataCount++;
                }
            }

            if(ratioDataCount > 0) // avoid division by 0
            {
                enemyRatio /= ratioDataCount;
            }

            //PersistenceUtil.WriteLine("Ratio   Enemy : " + enemyRatio);

            // compute the adjustment for the desired output
            double desiredAdjustment = 1 + (friendRatio - enemyRatio);

            //PersistenceUtil.WriteLine("Computed Output: " + m_computedOutput.ToString());
            //PersistenceUtil.WriteLine("Adjustment    : " + desiredAdjustment);

            // convert output and make adjustments to the desired and undesired outputs
            double[] output = new double[m_computedOutput.Count];
            for(int i = 0; i < m_computedOutput.Count; i++)
            {
                if(i == (int)m_randomOutput)
                {
                    output[i] = m_computedOutput[i] * desiredAdjustment;
                }
                else
                {
                    output[i] = m_computedOutput[i];
                }
            }

            // create and return the data pair made of the initial input information and 
            BasicMLDataPair pair = new BasicMLDataPair(new BasicMLData(m_inputInfo.GetNormalizedData()), new BasicMLData(output));
            //PersistenceUtil.WriteLine("Desired Output : " + pair.Ideal.ToString());
            //PersistenceUtil.WriteLine("<<< END FITNESS >>>");
            m_neuralNetController.AddTrainingDataPair(pair);
        }
        #endregion

        #region Local Functions
        #endregion
    }
}
