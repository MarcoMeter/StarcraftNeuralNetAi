using System.IO;
using BroodWar.Api;
using Encog.Neural.Networks;
using Encog.Persist;

namespace NeuralNetTraining.Utility
{
    /// <summary>
    /// The PersistenceUtil takes care of saving files such as log files and artificial neural network files. It also loads the last trained neural net on request.
    /// </summary>
    public class PersistenceUtil
    {
        #region Member
        private static string basePathLogs = @"bwapi-data\AI\cs\logs\";
        private static string basePathNets = @"bwapi-data\AI\cs\nets\";
        private static bool isSubDirLogsInit = false;
        private static string subDirLogs = "";
        private static string netFilePathInfo = basePathNets + "player" + Game.Self.Id.ToString() + "_LatestNet.txt";
        private static bool isSubDirNetsInit = false;
        private static string subDirNets = "";
        #endregion

        #region Public Functions
        /// <summary>
        /// Write line to logFile.log
        /// </summary>
        /// <param name="line">Content</param>
        public static void WriteLine(string line)
        {
            string fileName = @"\player" + Game.Self.Id.ToString() + "_match" + TrainingModule.GetMatchNumber() + ".log";

            // Create a logs folder
            if (!Directory.Exists(@"bwapi-data\AI\cs\logs"))
            {
                Directory.CreateDirectory(@"bwapi-data\AI\cs\logs");
            }

            // Create a new subfolder for the session
            if (!isSubDirLogsInit)
            {
                subDirLogs = basePathLogs + System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff") + "_player" + Game.Self.Id.ToString();
                Directory.CreateDirectory(subDirLogs);
                isSubDirLogsInit = true;
            }

            // Create and append to the log file
            using (StreamWriter file = new StreamWriter(subDirLogs + fileName, true))
            {
                file.WriteLine(System.DateTime.Now.TimeOfDay.ToString("") + "   " + line);
            }
        }

        /// <summary>
        /// A text file holds the path to the latest neural network file. Using that file path, the neural net gets loaded and returned.
        /// </summary>
        /// <returns>Returns the latest neural network.</returns>
        public static BasicNetwork GetLatestNeuralNet()
        {
            // If the nets directory or the file, which holds the reference to the path of the latest neural network, doesn't exist, return null and let the NeuralNetController instantiate a new neural network.
            if (!Directory.Exists(@"bwapi-data\AI\cs\nets") || !new FileInfo(netFilePathInfo).Exists)
            {
                return null;
            }

            // Read the path to the neural net line
            string latestNetFilePath = "";
            foreach (string line in File.ReadLines(netFilePathInfo))
            {
                latestNetFilePath = line;
            }

            // Instantiate a FileInfo to that specific file
            FileInfo neuralNetFile = new FileInfo(latestNetFilePath);

            // If that file doesn't exist return null, otherwise load the neural network.
            // In the case of not loading a neural net, the NeuralNetController will instantiate a new untrained one.
            if (neuralNetFile.Exists)
            {
                return (BasicNetwork)EncogDirectoryPersistence.LoadObject(neuralNetFile);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Writes the processed neural network to a file.
        /// </summary>
        /// <param name="net">The processed neural network.</param>
        public static void SaveNeuralNet(BasicNetwork net)
        {
            string fileName = @"\player" + Game.Self.Id.ToString() + "_match" + TrainingModule.GetMatchNumber() + ".ann";

            // Create a nets folder
            if (!Directory.Exists(@"bwapi-data\AI\cs\nets"))
            {
                Directory.CreateDirectory(@"bwapi-data\AI\cs\nets");
            }

            // Create a new subfolder for the session
            if (!isSubDirNetsInit)
            {
                subDirNets = basePathNets + System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff") + "_player" + Game.Self.Id.ToString();
                Directory.CreateDirectory(subDirNets);
                isSubDirNetsInit = true;
            }

            // Update the text file which keeps reference to the path of the latest neural network
            File.WriteAllText(netFilePathInfo, subDirNets + fileName);

            // Write neural net to file
            EncogDirectoryPersistence.SaveObject(new FileInfo(subDirNets + fileName), net);
        }
        #endregion
    }
}