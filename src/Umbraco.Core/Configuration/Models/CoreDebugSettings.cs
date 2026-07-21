// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for debug settings.
/// </summary>
// TODO (V19): Rename to DebugSettings to match the "Umbraco:CMS:Debug" section (the "Core" prefix
// dates from the legacy "Umbraco:CMS:Core:Debug" section, which is deprecated).
[UmbracoOptions(Constants.Configuration.ConfigDebug)]
public class CoreDebugSettings
{
    /// <summary>
    ///     The default value for logging incompleted scopes.
    /// </summary>
    internal const bool StaticLogIncompletedScopes = false;

    /// <summary>
    ///     The default value for dumping on timeout thread abort.
    /// </summary>
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
