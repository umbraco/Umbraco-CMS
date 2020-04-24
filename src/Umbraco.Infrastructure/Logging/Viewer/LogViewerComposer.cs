using System.IO;
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


            composition.RegisterUnique<ILoggingConfiguration>(factory =>
            {
                var hostingEnvironment = factory.GetInstance<IHostingEnvironment>();
                return new LoggingConfiguration(
                    Path.Combine(hostingEnvironment.ApplicationPhysicalPath, "App_Data\\Logs"),
                    Path.Combine(hostingEnvironment.ApplicationPhysicalPath, "config\\serilog.config"),
                    Path.Combine(hostingEnvironment.ApplicationPhysicalPath, "config\\serilog.user.config"));
            });
            composition.RegisterUnique<ILogViewerConfig, LogViewerConfig>();
            composition.SetLogViewer<SerilogJsonLogViewer>();
            composition.RegisterUnique<ILogViewer>(factory =>
            {

                return new SerilogJsonLogViewer(factory.GetInstance<ILogger>(),
                    factory.GetInstance<ILogViewerConfig>(),
                    factory.GetInstance<ILoggingConfiguration>(),
                    Log.Logger);
            } );
        }
    }
}
