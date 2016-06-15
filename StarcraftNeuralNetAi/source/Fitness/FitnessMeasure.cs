using Encog.ML.Data;
using Encog.ML.Data.Basic;
using NeuralNetTraining.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeuralNetTraining
{
    abstract public class FitnessMeasure
    {
        #region Member Fields
        protected InputVector m_initialInputVector;
        protected CombatUnitState m_randomOutput;
        protected IMLData m_computedOutput;
        protected NeuralNetController m_neuralNetController;
        #endregion

        #region Member Properties
        #endregion

        #region Constructor
        /// <summary>
        /// Based on the initial state of the combat, the random action to be executed and the computed outputs by the net, the FitnessMeasure is getting initialized.
        /// The actual fitness is measured after supplying the final state of the combat.
        /// </summary>
        /// <param name="inputVector">The initial input information, which was used to feed the neural net.</param>
        /// <param name="randomAction">The chosen action by random.</param>
        /// <param name="computedOutput">The computed outputs by the neural net.</param>
        public FitnessMeasure(InputVector inputVector, CombatUnitState randomAction, IMLData computedOutput)
        {
            this.m_initialInputVector = inputVector;
            this.m_randomOutput = randomAction;
            this.m_computedOutput = computedOutput;
            this.m_neuralNetController = NeuralNetController.Instance;
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
        abstract public void ComputeDataPair(InputVector finalInputVector, bool hit, bool killed);
        #endregion

        #region Local Functions
        #endregion
    }
}