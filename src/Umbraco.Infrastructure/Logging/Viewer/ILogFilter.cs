using Serilog.Events;

namespace Umbraco.Core.Logging.Viewer
{
    public interface ILogFilter
    {
        bool TakeLogEvent(LogEvent e);
    }
}
