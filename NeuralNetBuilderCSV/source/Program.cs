using Encog.App.Analyst;
using Encog.App.Analyst.CSV.Normalize;
using Encog.App.Analyst.Wizard;
using Encog.Engine.Network.Activation;
using Encog.ML.Data.Basic;
using Encog.ML.Data.Versatile;
using Encog.ML.Data.Versatile.Columns;
using Encog.ML.Data.Versatile.Sources;
using Encog.ML.Model;
using Encog.Neural.Networks;
using Encog.Neural.Networks.Layers;
using Encog.Neural.Networks.Training.Propagation.Back;
using Encog.Persist;
using Encog.Util.CSV;
using Encog.Util.Simple;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetBuilderCSV
{
    /// <summary>
    /// This console application reads a csv sheet with training examples which are getting fed to a neural network.
    /// </summary>
    class Program
    {
        #region Member Fields
        // Neural net structure
        private const int m_inputNodeCount = 5;
        private const int m_hiddenNodeCount = 10;
        private const int m_outputNodeCount = 3;
        // Training
        private const double errorThreshold = 0.01;
        #endregion

        #region Main
        static void Main(string[] args)
        {
            // Build neural net
            var neuralNet = BuildNeuralNet();

            // Load and prepare training data
            var trainingSet = EncogUtility.LoadCSV2Memory("trainingData.csv", neuralNet.InputCount, neuralNet.OutputCount, true, CSVFormat.English, false);

            // Train neural net
            var train = new Backpropagation(neuralNet, trainingSet);
            int epoch = 1;

            do
            {
                train.Iteration();
                Console.WriteLine(@"Epoch #" + epoch + @"  Error : " + train.Error);
                epoch++;
            } while (train.Error > errorThreshold);

            // Save neural net
            EncogDirectoryPersistence.SaveObject(new FileInfo("csvNet.ann"), neuralNet);
            
            Console.ReadLine(); // prevents the console from shutting down immediately
        }
        #endregion

        #region Local Functions
        private static BasicNetwork BuildNeuralNet()
        {
            var net = new BasicNetwork();
            net.AddLayer(new BasicLayer(null, true, m_inputNodeCount)); // input layer
            net.AddLayer(new BasicLayer(new ActivationSigmoid(), true, m_hiddenNodeCount)); // #1 hidden layer
            net.AddLayer(new BasicLayer(new ActivationSigmoid(), false, m_outputNodeCount)); // output layer
            net.Structure.FinalizeStructure();
            net.Reset(); // initializes the weights of the neural net
            return net;
        }
        #endregion
    }
}
