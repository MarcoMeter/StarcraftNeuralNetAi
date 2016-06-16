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
        private const double m_killWeight = 0.65;
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
                desiredMovementAdjustment = 1.5;
            }

            // Step #3: Build the new desired output vector
            double[] output = new double[m_computedOutput.Count];

            // Convert output and make adjustments to the desired and undesired outputs
            for (int i = 0; i < m_computedOutput.Count; i++)
            {
                if(i == (int)m_randomOutput)
                {
                    output[i] = m_computedOutput[i] * desiredOutputActionAdjustment;
                }
                else if(i == (int)CombatUnitState.MoveBack)
                {
                    output[i] = m_computedOutput[i] * desiredMovementAdjustment;
                }
                else
                {
                    output[i] = m_computedOutput[i];
                }
            }

            // Step #4: Create and forward the data pair made of the initial input vector and the adjusted output vector
            BasicMLDataPair pair = new BasicMLDataPair(new BasicMLData(m_initialInputVector.GetNormalizedData()), new BasicMLData(output));
            m_neuralNetController.AddTrainingDataPair(pair);

            PersistenceUtil.WriteLine("<<< START ATTACK FITNESS >>>");
            PersistenceUtil.WriteLine("action: " + m_randomOutput.ToString());
            PersistenceUtil.WriteLine("desired attack adjustment: " + desiredOutputActionAdjustment);
            PersistenceUtil.WriteLine("desired movement adjustment: " + desiredMovementAdjustment);
            PersistenceUtil.WriteLine("desired output: " + pair.Ideal.ToString());
            PersistenceUtil.WriteLine("Initial: " + String.Join(",", m_initialInputVector.GetNormalizedData().Select(p => p.ToString()).ToArray()));
            PersistenceUtil.WriteLine("Final: " + String.Join(",", finalInputVector.GetNormalizedData().Select(p => p.ToString()).ToArray()));
        }
        #endregion

        #region Local Functions
        #endregion
    }
}
