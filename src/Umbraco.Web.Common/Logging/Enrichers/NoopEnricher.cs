using Serilog.Core;
using Serilog.Events;

namespace Umbraco.Cms.Web.Common.Logging.Enrichers;

/// <summary>
///     NoOp but useful for tricks to avoid disposal of the global logger.
/// </summary>
internal class NoopEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
    }
}
