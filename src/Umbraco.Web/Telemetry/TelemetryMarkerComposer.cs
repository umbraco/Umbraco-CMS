using Umbraco.Core;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Telemetry
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Install, MaxLevel = RuntimeLevel.Upgrade)]
    public class TelemetryMarkerComposer : ComponentComposer<TelemetryMarkerComponent>, ICoreComposer
    { }
}
