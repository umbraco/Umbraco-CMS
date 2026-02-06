// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
/// Typed configuration options for basic authentication settings.
/// </summary>
[UmbracoOptions(Constants.Configuration.ConfigBasicAuth)]
public class BasicAuthSettings
{
    private const bool StaticEnabled = false;

    /// <summary>
    /// Gets or sets a value indicating whether Basic Auth Middleware is enabled.
    /// </summary>
    [DefaultValue(StaticEnabled)]
    public bool Enabled { get; set; } = StaticEnabled;

    /// <summary>
    ///     Gets or sets the collection of allowed IP addresses that bypass basic authentication.
    /// </summary>
    public ISet<string> AllowedIPs { get; set; } = new HashSet<string>();

    /// <summary>
    ///     Gets or sets the shared secret configuration for header-based authentication.
    /// </summary>
    public SharedSecret SharedSecret { get; set; } = new();

    /// <summary>
    ///     Gets or sets a value indicating whether to redirect to the login page instead of showing basic auth prompt.
    /// </summary>
    public bool RedirectToLoginPage { get; set; } = false;
}

/// <summary>
///     Typed configuration options for shared secret header-based authentication.
/// </summary>
public class SharedSecret
{
    private const string StaticHeaderName = "X-Authentication-Shared-Secret";

    /// <summary>
    ///     Gets or sets the name of the HTTP header containing the shared secret.
    /// </summary>
    [DefaultValue(StaticHeaderName)]
    public string? HeaderName { get; set; } = StaticHeaderName;

    /// <summary>
    ///     Gets or sets the shared secret value.
    /// </summary>
    public string? Value { get; set; }
}
