using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Umbraco.Core.Composing;
using Umbraco.Core.Hosting;

namespace Umbraco.Core.Logging.Viewer
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    // ReSharper disable once UnusedMember.Global
    public class LogViewerComposer : ICoreComposer
    {
        public void Compose(Composition composition)
        {
            composition.RegisterUnique<ILogViewerConfig, LogViewerConfig>();
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
