using System;
using System.IO;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;

namespace Umbraco.Web.Telemetry
{
    public class TelemetryMarkerComponent : IComponent
    {
        private IProfilingLogger _logger;
        private IRuntimeState _runtime;

        public TelemetryMarkerComponent(IProfilingLogger logger, IRuntimeState runtime)
        {
            _logger = logger;
            _runtime = runtime;
        }

        public void Initialize()
        {
            var telemetricsFilePath = IOHelper.MapPath(SystemFiles.TelemetricsIdentifier);

            // Verify file does not exist already (if we are upgrading)
            // In a clean install we know it would not exist
            // If the site is upgraded and the file was removed it would re-create one
            // NOTE: If user removed the marker file to opt out it would re-create a new guid marker file & potentially skew
            if (_runtime.Level == RuntimeLevel.Upgrade && File.Exists(telemetricsFilePath))
            {
                _logger.Warn<TelemetryMarkerComponent>("When upgrading the anonymous telemetry file already existsed on disk at {filePath}", telemetricsFilePath);
                return;
            }
            else if (_runtime.Level == RuntimeLevel.Install && File.Exists(telemetricsFilePath))
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
                _logger.Error<TelemetryMarkerComponent>(ex, "Unable to create telemetry file at {filePath}", telemetricsFilePath);
            }

        }

        public void Terminate()
        {
        }
    }
}
