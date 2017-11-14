using System;
using System.IO;
using log4net.Appender;

namespace Umbraco.Core.Logging
{
    public class CustomRollingFileAppender: RollingFileAppender
    {
        /// <summary>
        /// This ovveride will delete logs older than 30 days
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="append"></param>
        protected override void OpenFile(string fileName, bool append)
        {
            var logFolder = Path.GetDirectoryName(fileName);

            if (logFolder != null)
            {
                string[] logFiles = Directory.GetFiles(logFolder);

                foreach (var logFile in logFiles)
                {
                    DateTime lastAccessTime = System.IO.File.GetLastAccessTime(logFile);

                    if (lastAccessTime < DateTime.Now.AddMonths(-1))
                    {
                        System.IO.File.Delete(logFile);
                    }
                }
            }

            base.OpenFile(fileName, append);
        }

    }
}
