using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;

namespace Umbraco.Web.Telemetry
{
    public class TelemetryMarkerComponent : IComponent
    {
        private readonly ILogger<TelemetryMarkerComponent> _logger;
        private readonly IRuntimeState _runtime;
        private readonly IHostingEnvironment _hostingEnvironment;

        public TelemetryMarkerComponent(ILogger<TelemetryMarkerComponent> logger, IRuntimeState runtime, IHostingEnvironment hostingEnvironment)
        {
            _logger = logger;
            _runtime = runtime;
            _hostingEnvironment = hostingEnvironment;
        }

        public void Initialize()
        {
            if (_runtime.Level != RuntimeLevel.Install && _runtime.Level != RuntimeLevel.Upgrade)
            {
                return;
            }

            var telemetricsFilePath = _hostingEnvironment.MapPathContentRoot(SystemFiles.TelemetricsIdentifier);

            // Verify file does not exist already (if we are upgrading)
            // In a clean install we know it would not exist
            // If the site is upgraded and the file was removed it would re-create one
            // NOTE: If user removed the marker file to opt out it would re-create a new guid marker file & potentially skew
            if (_runtime.Level == RuntimeLevel.Upgrade && File.Exists(telemetricsFilePath))
            {
                _logger.LogWarning("When upgrading the anonymous telemetry file already existed on disk at {filePath}", telemetricsFilePath);
                return;
            }
            if (_runtime.Level == RuntimeLevel.Install && File.Exists(telemetricsFilePath))
            {
                // No need to log for when level is install if file exists (As this component hit several times during install process)
                return;
            }

            // We are a clean install or an upgrade without the marker file
            // Generate GUID
            var telemetrySiteIdentifier = Guid.NewGuid();

            // Write file contents
            try
            {
                File.WriteAllText(telemetricsFilePath, telemetrySiteIdentifier.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to create telemetry file at {filePath}", telemetricsFilePath);
            }



        }

        public void Terminate()
        {
        }
    }
}
