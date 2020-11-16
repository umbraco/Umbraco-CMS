using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Umbraco.Core.Composing;

namespace Umbraco.Core.Logging.Viewer
{
    // ReSharper disable once UnusedMember.Global
    public class LogViewerComposer : ICoreComposer
    {
        public void Compose(Composition composition)
        {
            composition.Services.AddUnique<ILogViewerConfig, LogViewerConfig>();
            composition.SetLogViewer<SerilogJsonLogViewer>();
            composition.Services.AddUnique<ILogViewer>(factory =>
            {

                return new SerilogJsonLogViewer(factory.GetRequiredService<ILogger<SerilogJsonLogViewer>>(),
                    factory.GetRequiredService<ILogViewerConfig>(),
                    factory.GetRequiredService<ILoggingConfiguration>(),
                    Log.Logger);
            } );
        }
    }
}
