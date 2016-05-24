using BroodWar.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AttackAnalysisModule
{
    public class LogUtil
    {
        #region Member Fields
        private static List<string> m_logStrings = new List<string>();
        #endregion

        #region Public Functions
        /// <summary>
        /// Add a line to the list of stirngs in order to log.
        /// </summary>
        /// <param name="line">Line to log</param>
        public static void AppendString(string line)
        {
            if (line != null)
            {
                m_logStrings.Add(line);
            }
        }

        /// <summary>
        /// Writes all saved strings to a csv file.
        /// </summary>
        public static void FinalizeLog()
        {
            if(m_logStrings.Count > 0)
            {
                foreach(string line in m_logStrings)
                {
                    WriteLine(line);
                }
            }
        }
        #endregion

        #region Private Functions
        /// <summary>
        /// Writes line to log file.
        /// </summary>
        /// <param name="line">The line to write.</param>
        private static void WriteLine(string line)
        {
            string fileName = @"bwapi-data\AI\cs\attack.csv";

            // Create and append to the log file
            using (StreamWriter file = new StreamWriter(fileName, true))
            {
                file.WriteLine(line);
            }
        }
        #endregion
    }
}
