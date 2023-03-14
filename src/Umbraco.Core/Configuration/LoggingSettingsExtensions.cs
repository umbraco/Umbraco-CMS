// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Hosting;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Extensions;

namespace Umbraco.Extensions;

/// <summary>
/// Extension methods for <see cref="LoggingSettings" />.
/// </summary>
public static class LoggingSettingsExtensions
{
    /// <summary>
    /// Gets the absolute logging path (maps a virtual path to the applications content root).
    /// </summary>
    /// <param name="loggingSettings">The logging settings.</param>
    /// <param name="hostEnvironment">The host environment.</param>
    /// <returns>
    /// The absolute logging path.
    /// </returns>
    public static string GetAbsoluteLoggingPath(this LoggingSettings loggingSettings, IHostEnvironment hostEnvironment)
    {
        var dir = loggingSettings.Directory;
        if (dir.StartsWith("~/"))
        {
            return hostEnvironment.MapPathContentRoot(dir);
        }

        return dir;
    }
}
