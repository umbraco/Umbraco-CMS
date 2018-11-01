using System;
using System.Collections.Generic;
using System.IO;
using Serilog.Events;
using Serilog.Formatting.Compact.Reader;

namespace Umbraco.Core.Logging.Viewer
{
    public partial class JsonLogViewer : LogViewerSourceBase
    {       
        public override IEnumerable<LogEvent> GetLogs(DateTimeOffset startDate, DateTimeOffset endDate, ILogFilter filter, int skip, int take)
        {
            var logs = new List<LogEvent>();

            //Open the JSON log file for the range of dates (and exclude machinename) Could be several for LB
            var dateRange = endDate - startDate;

            //Log Directory
            var logDirectory = $@"{AppDomain.CurrentDomain.BaseDirectory}\App_Data\Logs\";

            var count = 0;

            //foreach full day in the range - see if we can find one or more filenames that end with
            //yyyyMMdd.json - Ends with due to MachineName in filenames - could be 1 or more due to load balancing
            for (var day = startDate.Date; day.Date <= endDate.Date; day = day.AddDays(1))
            {
                //Filename ending to search for (As could be multiple)
                var filesToFind = $"*{day.ToString("yyyyMMdd")}.json";

                var filesForCurrentDay = Directory.GetFiles(logDirectory, filesToFind);

                //Foreach file we find - open it
                foreach (var filePath in filesForCurrentDay)
                {
                    //Open log file & add contents to the log collection
                    //Which we then use LINQ to page over
                    using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        using (var stream = new StreamReader(fs))
                        {
                            var reader = new LogEventReader(stream);
                            LogEvent evt;
                            while (reader.TryRead(out evt))
                            {
                                //TODO - convert psuedo code
                                if (count > skip + take)
                                {
                                    break;
                                }

                                if (count < skip)
                                {
                                    count++;
                                    continue;
                                }

                                if (filter.TakeLogEvent(evt))
                                {
                                    logs.Add(evt);
                                }
                                    
                                count++;
                            }
                        }
                    }
                }
            }

            return logs;
        }

        /// <summary>
        /// This default JSON disk implementation here - returns the total filesize & NOT the count of entries
        /// Other implementations we would expect to return the count of entries
        /// We use this number to help prevent the logviewer killing the site with CPU/Memory if the number of items too big to handle
        /// </summary>
        public override long GetLogSize(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            //Open the JSON log file for the range of dates (and exclude machinename) Could be several for LB
            var dateRange = endDate - startDate;

            //Log Directory
            var logDirectory = $@"{AppDomain.CurrentDomain.BaseDirectory}\App_Data\Logs\";

            //Number of entries
            long count = 0;

            //foreach full day in the range - see if we can find one or more filenames that end with
            //yyyyMMdd.json - Ends with due to MachineName in filenames - could be 1 or more due to load balancing
            for (var day = startDate.Date; day.Date <= endDate.Date; day = day.AddDays(1))
            {
                //Filename ending to search for (As could be multiple)
                var filesToFind = $"*{day.ToString("yyyyMMdd")}.json";

                var filesForCurrentDay = Directory.GetFiles(logDirectory, filesToFind);

                //Foreach file we find - open it
                foreach (var filePath in filesForCurrentDay)
                {
                    //Get the current filesize in bytes !
                    var byteFileSize = new FileInfo(filePath).Length;

                    count += byteFileSize;
                }
            }

            //Count contains a combination of file sizes in bytes
            return count;
        }
    }
}
