using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
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

        private static readonly IEnumerable<TelemetryData> s_data = new[]
        {
            TelemetryData.CustomGlobalSettings,
            TelemetryData.ModelsBuilderMode
        };

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
        public IEnumerable<TelemetryData> Data => s_data;

        /// <inheritdoc/>
        public object Collect(TelemetryData telemetryData) => telemetryData switch
        {
            TelemetryData.CustomGlobalSettings => GetCustomGlobalSettings(),
            TelemetryData.ModelsBuilderMode => _modelsBuilderSettings.CurrentValue.ModelsMode,
            _ => throw new NotSupportedException()
        };

        private CustomGlobalSettings GetCustomGlobalSettings()
        {
            var globalSettings = _globalSettings.CurrentValue;

            return new()
            {
                TimeOut = globalSettings.TimeOut,
                DefaultUILanguage = globalSettings.DefaultUILanguage,
                HideTopLevelNodeFromPath = globalSettings.HideTopLevelNodeFromPath,
                UseHttps = globalSettings.UseHttps,
                UmbracoPath = globalSettings.UmbracoPath != GlobalSettings.StaticUmbracoPath,
                IconsPath = globalSettings.IconsPath != GlobalSettings.StaticIconsPath,
                UmbracoCssPath = globalSettings.UmbracoCssPath != GlobalSettings.StaticUmbracoCssPath,
                UmbracoScriptsPath = globalSettings.UmbracoScriptsPath != GlobalSettings.StaticUmbracoScriptsPath,
                UmbracoMediaPath = globalSettings.UmbracoMediaPath != GlobalSettings.StaticUmbracoMediaPath,
                UmbracoMediaPhysicalRootPath = globalSettings.UmbracoMediaPhysicalRootPath != GlobalSettings.StaticUmbracoMediaPath,
                MainDomLock = globalSettings.MainDomLock,
                IsSmtpServerConfigured = globalSettings.IsSmtpServerConfigured,
                IsPickupDirectoryLocationConfigured = globalSettings.IsPickupDirectoryLocationConfigured,
                SqlWriteLockTimeOut = globalSettings.SqlWriteLockTimeOut
            };
        }

        [DataContract]
        private class CustomGlobalSettings
        {
            [DataMember(Name = "timeOut")]
            public TimeSpan TimeOut { get; set; }

            [DataMember(Name = "defaultUILanguage")]
            public string DefaultUILanguage { get; set; }

            [DataMember(Name = "hideTopLevelNodeFromPath")]
            public bool HideTopLevelNodeFromPath { get; set; }

            [DataMember(Name = "useHttps")]
            public bool UseHttps { get; set; }

            [DataMember(Name = "umbracoPath")]
            public bool UmbracoPath { get; set; }

            [DataMember(Name = "iconsPath")]
            public bool IconsPath { get; set; }

            [DataMember(Name = "umbracoCssPath")]
            public bool UmbracoCssPath { get; set; }

            [DataMember(Name = "umbracoScriptsPath")]
            public bool UmbracoScriptsPath { get; set; }

            [DataMember(Name = "umbracoMediaPath")]
            public bool UmbracoMediaPath { get; set; }

            [DataMember(Name = "umbracoMediaPhysicalRootPath")]
            public bool UmbracoMediaPhysicalRootPath { get; set; }

            [DataMember(Name = "mainDomLock")]
            public string MainDomLock { get; set; }

            [DataMember(Name = "isSmtpServerConfigured")]
            public bool IsSmtpServerConfigured { get; set; }

            [DataMember(Name = "isPickupDirectoryLocationConfigured")]
            public bool IsPickupDirectoryLocationConfigured { get; set; }

            [DataMember(Name = "sqlWriteLockTimeOut")]
            public TimeSpan SqlWriteLockTimeOut { get; set; }
        }
    }
}
