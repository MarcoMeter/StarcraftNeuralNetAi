using Encog.ML.Data;
using NeuralNetTraining.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeuralNetTraining
{
    /// <summary>
    /// The MovementFitness measures the fitness of movement actions.
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
        /// After that, the desired output gets computed and a data pair based on the initial input with the new desired output is getting sent to the neural net controller.
        /// </summary>
        /// <param name="finalInputInfo">Add the final InputInformation in order to compare the initial and final state of the combat which has been altered after executing an action.</param>
        /// <param name="hit">In the case of movement enter false</param>
        /// <param name="killed">In the case of movement enter false</param>
        public override void ComputeDataPair(InputVector finalInputVector, bool hit, bool killed)
        {
            
        }
        #endregion

        #region Local Functions
        #endregion
    }
}
