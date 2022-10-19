namespace Umbraco.Cms.Core.Logging.Viewer;

public class LogTimePeriod
{
    public LogTimePeriod(DateTime startTime, DateTime endTime)
    {
        StartTime = startTime;
        EndTime = endTime;
    }

    public DateTime StartTime { get; }

    public DateTime EndTime { get; }
}
