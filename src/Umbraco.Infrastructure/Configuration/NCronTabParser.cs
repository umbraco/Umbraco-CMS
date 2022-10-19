using NCrontab;

namespace Umbraco.Cms.Core.Configuration;

/// <summary>
/// Implements <see cref="ICronTabParser"/> using the NCrontab library
/// </summary>
public class NCronTabParser : ICronTabParser
{
    /// <inheritdoc/>
    public bool IsValidCronTab(string cronTab)
    {
        var result = CrontabSchedule.TryParse(cronTab);

        return !(result is null);
    }

    /// <inheritdoc/>
    public DateTime GetNextOccurrence(string cronTab, DateTime time)
    {
        var result = CrontabSchedule.Parse(cronTab);

        return result.GetNextOccurrence(time);
    }
}
