using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Hosting;
using Umbraco.Cms.Core.Telemetry.Models;

namespace Umbraco.Cms.Core.Telemetry.DataCollectors
{
    /// <summary>
    /// Collects system information telemetry data.
    /// </summary>
    /// <seealso cref="Umbraco.Cms.Core.Telemetry.ITelemetryDataCollector" />
    internal class SystemInformationTelemetryDataCollector : ITelemetryDataCollector
    {
        private readonly IHostEnvironment _hostEnvironment;

        private static readonly IEnumerable<TelemetryData> s_data = new[]
        {
            TelemetryData.OS,
            TelemetryData.OSArchitecture,
            TelemetryData.ProcessArchitecture,
            TelemetryData.Framework,
            TelemetryData.Server,
            TelemetryData.EnvironmentName
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemInformationTelemetryDataCollector" /> class.
        /// </summary>
        public SystemInformationTelemetryDataCollector(IHostEnvironment hostEnvironment) => _hostEnvironment = hostEnvironment;

        /// <inheritdoc/>
        public IEnumerable<TelemetryData> Data => s_data;

        /// <inheritdoc/>
        public object Collect(TelemetryData telemetryData) => telemetryData switch
        {
            TelemetryData.OS => RuntimeInformation.OSDescription,
            TelemetryData.OSArchitecture => RuntimeInformation.OSArchitecture,
            TelemetryData.ProcessArchitecture => RuntimeInformation.ProcessArchitecture,
            TelemetryData.Framework => RuntimeInformation.FrameworkDescription,
            TelemetryData.Server => GetServer(),
            TelemetryData.EnvironmentName => _hostEnvironment.EnvironmentName,
            _ => throw new NotSupportedException()
        };

        internal static string GetServer()
        {
            using var process = Process.GetCurrentProcess();

            return process.ProcessName switch
            {
                "w3wp" => "IIS",
                "iisexpress" => "IIS Express",
                _ => "Kestrel"
            };
        }
    }
}
