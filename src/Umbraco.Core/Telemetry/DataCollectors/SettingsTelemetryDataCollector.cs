using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Telemetry.Models;

namespace Umbraco.Cms.Core.Telemetry.DataCollectors
{
    /// <summary>
    /// Collects settings telemetry data.
    /// </summary>
    /// <seealso cref="Umbraco.Cms.Core.Telemetry.ITelemetryDataCollector" />
    internal class SettingsTelemetryDataCollector : ITelemetryDataCollector
    {
        private readonly IOptionsMonitor<GlobalSettings> _globalSettings;
        private readonly IOptionsMonitor<ModelsBuilderSettings> _modelsBuilderSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsTelemetryDataCollector" /> class.
        /// </summary>
        public SettingsTelemetryDataCollector(
            IOptionsMonitor<GlobalSettings> globalSettings,
            IOptionsMonitor<ModelsBuilderSettings> modelsBuilderSettings)
        {
            _globalSettings = globalSettings;
            _modelsBuilderSettings = modelsBuilderSettings;
        }

        /// <inheritdoc/>
        public IEnumerable<TelemetryData> Data => new[]
        {
            TelemetryData.CustomGlobalSettings,
            TelemetryData.ModelsBuilderMode
        };

        /// <inheritdoc/>
        public object Collect(TelemetryData telemetryData) => telemetryData switch
        {
            TelemetryData.CustomGlobalSettings => GetCustomGlobalSettings(),
            TelemetryData.ModelsBuilderMode => _modelsBuilderSettings.CurrentValue.ModelsMode,
            _ => throw new NotSupportedException()
        };
        private object GetCustomGlobalSettings()
        {
            var globalSettings = _globalSettings.CurrentValue;

            return new
            {
                globalSettings.TimeOut,
                globalSettings.DefaultUILanguage,
                globalSettings.HideTopLevelNodeFromPath,
                globalSettings.UseHttps,
                UmbracoPath = globalSettings.UmbracoPath != GlobalSettings.StaticUmbracoPath,
                IconsPath = globalSettings.IconsPath != GlobalSettings.StaticIconsPath,
                UmbracoCssPath = globalSettings.UmbracoCssPath != GlobalSettings.StaticUmbracoCssPath,
                UmbracoScriptsPath = globalSettings.UmbracoScriptsPath != GlobalSettings.StaticUmbracoScriptsPath,
                UmbracoMediaPath = globalSettings.UmbracoMediaPath != GlobalSettings.StaticUmbracoMediaPath,
                UmbracoMediaPhysicalRootPath = globalSettings.UmbracoMediaPhysicalRootPath != GlobalSettings.StaticUmbracoMediaPath,
                globalSettings.MainDomLock,
                globalSettings.IsSmtpServerConfigured,
                globalSettings.IsPickupDirectoryLocationConfigured,
                globalSettings.SqlWriteLockTimeOut
            };
        }
    }
}
