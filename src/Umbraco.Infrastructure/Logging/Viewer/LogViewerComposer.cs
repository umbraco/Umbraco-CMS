using System.IO;
using Microsoft.Extensions.DependencyInjection;
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


            composition.Services.AddUnique<ILoggingConfiguration>(factory =>
            {
                var hostingEnvironment = factory.GetRequiredService<IHostingEnvironment>();
                return new LoggingConfiguration(
                    Path.Combine(hostingEnvironment.ApplicationPhysicalPath, "App_Data", "Logs"),
                    Path.Combine(hostingEnvironment.ApplicationPhysicalPath, "config", "serilog.config"),
                    Path.Combine(hostingEnvironment.ApplicationPhysicalPath, "config", "serilog.user.config"));
            });
            composition.RegisterUnique<ILogViewerConfig, LogViewerConfig>();
            composition.SetLogViewer<SerilogJsonLogViewer>();
            composition.Services.AddUnique<ILogViewer>(factory =>
            {

                return new SerilogJsonLogViewer(factory.GetRequiredService<ILogger>(),
                    factory.GetRequiredService<ILogViewerConfig>(),
                    factory.GetRequiredService<ILoggingConfiguration>(),
                    Log.Logger);
            } );
        }
    }
}
