using System.IO;
using BroodWar.Api;

namespace NetworkTraining
{
    public class FileLogUtil
    {
        private static string path = @"bwapi-data\AI\cs\logFile" + Game.Self.Id.ToString() + ".txt";
        
        /// <summary>
        /// Write line to logFile.log
        /// </summary>
        /// <param name="line"></param>
        public static void WriteLine(string line)
        {
            using (StreamWriter file = new StreamWriter(path, true))
            {
                file.WriteLine(System.DateTime.Now.TimeOfDay.ToString() + "   " + line);
            }
        }
    }
}
