using System.IO;
using BroodWar.Api;

namespace NetworkTraining
{
    public class FileLogUtil
    {
        #region Member
        private static string basePath = @"bwapi-data\AI\cs\logs\";
        private static bool isSubDirInit = false;
        private static string subDir = "";
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
            if (!isSubDirInit)
            {
                subDir = basePath + @"\player" + Game.Self.Id.ToString() + "_" + System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff");
                Directory.CreateDirectory(subDir);
                isSubDirInit = true;
            }

            // Create and append to the log file
            using (StreamWriter file = new StreamWriter(subDir + fileName, true))
            {
                file.WriteLine(System.DateTime.Now.TimeOfDay.ToString("") + "   " + line);
            }
        }
        #endregion
    }
}