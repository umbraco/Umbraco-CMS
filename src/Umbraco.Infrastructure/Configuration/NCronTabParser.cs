using System;
using NCrontab;
using Umbraco.Cms.Core.Configuration;

namespace Umbraco.Core.Configuration
{
    public class NCronTabParser : ICronTabParser
    {
        public bool IsValidCronTab(string cronTab)
        {
            var result = CrontabSchedule.TryParse(cronTab);

            return !(result is null);
        }

        public DateTime GetNextOccurrence(string cronTab, DateTime time)
        {
            var result = CrontabSchedule.Parse(cronTab);

            return result.GetNextOccurrence(time);
        }
    }

}
