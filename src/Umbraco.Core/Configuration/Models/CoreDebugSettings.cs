// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Core.Configuration.Models
{
    /// <summary>
    /// Typed configuration options for core debug settings.
    /// </summary>
    public class CoreDebugSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether incompleted scopes should be logged.
        /// </summary>
        public bool LogIncompletedScopes { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether memory dumps on thread abort should be taken.
        /// </summary>
        public bool DumpOnTimeoutThreadAbort { get; set; } = false;
    }
}
