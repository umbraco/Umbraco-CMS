using System.IO;
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
            composition.RegisterUnique<ILogViewer>(factory =>
            {

                return new SerilogJsonLogViewer(factory.GetInstance<ILogger<SerilogJsonLogViewer>>(),
                    factory.GetInstance<ILogViewerConfig>(),
                    factory.GetInstance<ILoggingConfiguration>(),
                    Log.Logger);
            } );
        }
    }
}
