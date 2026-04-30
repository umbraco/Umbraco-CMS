using Serilog.Events;

namespace Umbraco.Cms.Core.Logging.Viewer;

internal sealed class MessageTemplateFilter : ILogFilter
{
    public readonly Dictionary<string, int> Counts = new();

    /// <summary>
    /// Evaluates the specified log event, updates the internal count for its message template, and determines whether it should be taken.
    /// </summary>
    /// <param name="e">The log event to evaluate.</param>
    /// <returns>
    /// Always returns <c>false</c>, indicating that the log event should not be taken, but updates the count for the event's message template.
    /// </returns>
    public bool TakeLogEvent(LogEvent e)
    {
        var templateText = e.MessageTemplate.Text;
        if (Counts.TryGetValue(templateText, out var count))
        {
            count++;
        }
        else
        {
            count = 1;
        }

        Counts[templateText] = count;

        // Don't add it to the list
        return false;
    }
}
