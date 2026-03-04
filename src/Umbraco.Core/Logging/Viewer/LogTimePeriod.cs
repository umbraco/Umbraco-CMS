namespace Umbraco.Cms.Core.Logging.Viewer;

/// <summary>
///     Represents a time period for filtering log entries.
/// </summary>
public class LogTimePeriod
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="LogTimePeriod"/> class.
    /// </summary>
    /// <param name="startTime">The start time of the period.</param>
    /// <param name="endTime">The end time of the period.</param>
    public LogTimePeriod(DateTime startTime, DateTime endTime)
    {
        StartTime = startTime;
        EndTime = endTime;
    }

    /// <summary>
    ///     Gets the start time of the period.
    /// </summary>
    public DateTime StartTime { get; }

    /// <summary>
    ///     Gets the end time of the period.
    /// </summary>
    public DateTime EndTime { get; }
}
