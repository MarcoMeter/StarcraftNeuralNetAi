using Encog.ML.Data;
using NeuralNetTraining.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeuralNetTraining
{
    public class AttackFitness : FitnessMeasure
    {
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
            
        }
        #endregion

        #region Local Functions
        #endregion
    }
}
