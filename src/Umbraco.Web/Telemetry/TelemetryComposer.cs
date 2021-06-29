using Umbraco.Core;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Telemetry
{

    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class TelemetryComposer : ComponentComposer<TelemetryComponent>, ICoreComposer
    { }
}
