// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for core debug settings.
/// </summary>
[UmbracoOptions(Constants.Configuration.ConfigCoreDebug)]
public class CoreDebugSettings
{
    internal const bool StaticLogIncompletedScopes = false;
    internal const bool StaticDumpOnTimeoutThreadAbort = false;

    /// <summary>
    ///     Gets or sets a value indicating whether incompleted scopes should be logged.
    /// </summary>
    [DefaultValue(StaticLogIncompletedScopes)]
    public bool LogIncompletedScopes { get; set; } = StaticLogIncompletedScopes;

    /// <summary>
    ///     Gets or sets a value indicating whether memory dumps on thread abort should be taken.
    /// </summary>
    [DefaultValue(StaticDumpOnTimeoutThreadAbort)]
    public bool DumpOnTimeoutThreadAbort { get; set; } = StaticDumpOnTimeoutThreadAbort;
}
