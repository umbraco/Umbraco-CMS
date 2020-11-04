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

            // Verify file does not exist already
            // If the site is upgraded and the file was removed it would re-create one
            if (File.Exists(telemetricsFilePath))
            {
                _logger.Warn<TelemetryMarkerComponent>("When installing or upgrading the anonymous telemetry file already existsed on disk at {filePath} with the runtime state {runtimeStateLevel}", telemetricsFilePath, _runtime.Level);
                return;
            }

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
