using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BroodWar.Api;
using Encog.Neural.Networks;
using NeuralNetTraining.Utility;
using Encog.Neural.Networks.Layers;
using Encog.Engine.Network.Activation;
using Encog.Neural.Networks.Training.Propagation.Back;
using Encog.ML.Data.Basic;
using Encog.Persist;

namespace NeuralNetTraining
{
    /// <summary>
    /// The NeuralNetController provides the support for Neural Networks. It takes care of providing the neural nets to the combat units and processes the generated training data.
    /// </summary>
    public class NeuralNetController
    {
        #region Member
        private static NeuralNetController instance;
        private BasicMLDataSet dataSet = new BasicMLDataSet(); // stores all the training examples of one match
        private BasicNetwork neuralNet;
        private Backpropagation trainer;
        private double learningRate = 0.1;
        private double momentum = 0.1;
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
                this.neuralNet.AddLayer(new BasicLayer(null, true, 2)); // Input layer
                this.neuralNet.AddLayer(new BasicLayer(new ActivationSigmoid(), true, 10)); // #1 hidden layer
                this.neuralNet.AddLayer(new BasicLayer(new ActivationSigmoid(), false, 3)); // output layer
                this.neuralNet.Structure.FinalizeStructure();
                this.neuralNet.Reset(); // initializes the weights of the neural net
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
        public void AddTrainingDataPair(BasicMLDataPair dataPair)
        {
            dataSet.Add(dataPair);
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
        /// Starts iterating through the gathered training examples in order to train the neural net. This should be called as soon as a match concludes.
        /// </summary>
        public void ExecuteTraining()
        {
            if (dataSet.Count > 0)
            {
                trainer = new Backpropagation(neuralNet, dataSet, learningRate, momentum);
                trainer.Iteration(); // iterating just once over the gathered data is required. That's because of the training examples will improve over time. The early produced examples aren't as ideal as the later ones.
                trainer.FinishTraining();
                dataSet = new BasicMLDataSet();
                PersistenceUtil.WriteLine("ERROR Rate : " + trainer.Error);
                PersistenceUtil.SaveNeuralNet(neuralNet);
            }
        }
        #endregion

        #region Local Functions
        #endregion
    }
}
