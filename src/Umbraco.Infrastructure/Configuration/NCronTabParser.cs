using NCrontab;

namespace Umbraco.Cms.Core.Configuration;

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
