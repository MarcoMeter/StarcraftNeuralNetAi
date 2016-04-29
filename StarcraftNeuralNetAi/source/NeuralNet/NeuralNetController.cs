using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BroodWar.Api;
using Encog.Neural.Networks;
using Encog.Persist;
using NeuralNetTraining.Utility;
using Encog.Neural.Networks.Layers;
using Encog.Engine.Network.Activation;

namespace NeuralNetTraining
{
    /// <summary>
    /// The NeuralNetController provides the support for Neural Networks. It takes care of providing the neural nets to the combat units and processes the generated training data.
    /// </summary>
    public class NeuralNetController
    {
        #region Member
        private static NeuralNetController instance;
        private List<double[]> trainingData = new List<double[]>();
        private BasicNetwork neuralNet;

        #endregion

        #region Constructor
        /// <summary>
        /// DUring the instantiation process, the NeuralNetController reads text file in order to retrieve the path to the latest neural network file, which is then loaded.
        /// </summary>
        private  NeuralNetController()
        {
            // load the latest neural network
            this.neuralNet = PersistenceUtil.GetLatestNeuralNet();

            // if no neural net exists, create one
            if(this.neuralNet == null)
            {
                this.neuralNet = new BasicNetwork();
                this.neuralNet.AddLayer(new BasicLayer(null, true, 9));
                this.neuralNet.AddLayer(new BasicLayer(new ActivationSigmoid(), true, 32));
                this.neuralNet.AddLayer(new BasicLayer(new ActivationSigmoid(), true, 32));
                this.neuralNet.AddLayer(new BasicLayer(new ActivationSigmoid(), false, 6));
                this.neuralNet.Structure.FinalizeStructure();
                this.neuralNet.Reset();
            }
        }
        #endregion

        #region Public Functions
        /// <summary>
        /// The NeuralNetController is a Singleton. If an instance hasn't been created yet, a new one will be instantiated and returned.
        /// </summary>
        /// <returns>Returns the only instance of the NeuralNetController</returns>
        public static NeuralNetController GetInstance()
        {
            if(instance == null)
            {
                instance = new NeuralNetController();
            }

            return instance;
        }

        /// <summary>
        /// Adds the generated data pairs to the training data list.
        /// </summary>
        /// <param name="dataPair">A data pair made of the normalized input information and the ideal output action.</param>
        public void AddTrainingDataPair(double[] dataPair)
        {
            trainingData.Add(dataPair);
        }

        /// <summary>
        /// Getter for the current artificial neural network.
        /// </summary>
        /// <returns>Returns the current neural net.</returns>
        public BasicNetwork GetNeuralNet()
        {
            return this.neuralNet;
        }

        /// <summary>
        /// Starts iterating through the gathered training examples in order to train the neural net. This should be called as soon a match concludes (preferably by the SquadSupervisor).
        /// </summary>
        public void ExecuteTraining()
        {
            PersistenceUtil.SaveNeuralNet(neuralNet);
        }
        #endregion

        #region Local Functions

        #endregion
    }
}
