using System;
using System.IO;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;


namespace Umbraco.Core.Migrations.Upgrade.V_8_10_0
{

    public class SetupAnonInstallTracker : MigrationBase
    {
        public SetupAnonInstallTracker(IMigrationContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Adds a new file 'telemetrics-id.umb' at /umbraco
        /// Which will add a GUID inside the file as JSON
        /// </summary>
        public override void Migrate()
        {
            var telemetricsFilePath = IOHelper.MapPath(SystemFiles.TelemetricsIdentifier);

            // Verify file does not exist already
            if (File.Exists(telemetricsFilePath))
            {
                Logger.Warn<SetupAnonInstallTracker>("When migrating to 8.10.0 the anonymous telemetry file already existsed on disk at {filePath}", telemetricsFilePath);
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
                Logger.Error<SetupAnonInstallTracker>(ex, "Unable to create telemetry file at {filePath}", telemetricsFilePath);
            }
            
            Logger.Info<SetupAnonInstallTracker>("This site has been identified with an anynomous id {telemetrySiteId} for telemetrics and written to {filePath}", telemetrySiteIdentifier, telemetricsFilePath);

        }
    }
}
