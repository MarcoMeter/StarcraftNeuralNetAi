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
        #region Member Fields
        // Singleton
        private static NeuralNetController m_instance;

        // Neural Net
        private BasicNetwork m_neuralNet;

        // Training fields
        private BasicMLDataSet m_dataSet = new BasicMLDataSet(); // stores all the training examples of one match
        private Backpropagation m_trainer;
        private double m_learningRate = 0.1;
        private double m_momentum = 0.1;
        private double m_iterationMultiplier = 2;
        #endregion

        #region Member Properties
        /// <summary>
        /// Read-only. The NeuralNetController is a Singleton. If an instance hasn't been created yet, a new one will be instantiated and returned.
        /// </summary>
        public static NeuralNetController Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new NeuralNetController();
                }

                return m_instance;
            }
        }

        /// <summary>
        /// Read-only property for the current artificial neural network.
        /// </summary>
        public BasicNetwork NeuralNet
        {
            get
            {
                return this.m_neuralNet;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// During the instantiation process, the NeuralNetController reads text file in order to retrieve the path to the latest neural network file, which is then loaded.
        /// </summary>
        private NeuralNetController()
        {
            // load the latest neural network
            this.m_neuralNet = PersistenceUtil.GetLatestNeuralNet();

            // if no neural net exists, create one
            if(this.m_neuralNet == null)
            {
                this.m_neuralNet = new BasicNetwork();
                this.m_neuralNet.AddLayer(new BasicLayer(null, true, 18)); // input layer
                this.m_neuralNet.AddLayer(new BasicLayer(new ActivationSigmoid(), true, 28)); // #1 hidden layer
                this.m_neuralNet.AddLayer(new BasicLayer(new ActivationSigmoid(), false, 3)); // output layer
                this.m_neuralNet.Structure.FinalizeStructure();
                this.m_neuralNet.Reset(); // initializes the weights of the neural net
            }
        }
        #endregion

        #region Public Functions
        /// <summary>
        /// Adds the generated data pairs to the training data list.
        /// </summary>
        /// <param name="dataPair">A data pair made of the normalized input information and the ideal output action.</param>
        public void AddTrainingDataPair(BasicMLDataPair dataPair)
        {
            if (dataPair != null)
            {
                m_dataSet.Add(dataPair);
            }
        }

        /// <summary>
        /// Starts iterating through the gathered training examples in order to train the neural net. This should be called as soon as a match concludes.
        /// </summary>
        public void ExecuteTraining()
        {
            if (m_dataSet.Count > 0)
            {
                m_trainer = new Backpropagation(m_neuralNet, m_dataSet);
                for (int i = 0; i <= TrainingModule.MatchNumber * m_iterationMultiplier; i++)
                {
                    m_trainer.Iteration(); // iterating just once over the gathered data is required. That's because of the training examples will improve over time. The early produced examples aren't as ideal as the later ones.
                }
                m_trainer.FinishTraining();
                PersistenceUtil.WriteLine("Training Examples count : " + m_dataSet.Count);
                m_dataSet = new BasicMLDataSet();
                PersistenceUtil.WriteLine("ERROR Rate : " + m_trainer.Error);
                PersistenceUtil.SaveNeuralNet(m_neuralNet);
            }
        }
        #endregion
    }
}
