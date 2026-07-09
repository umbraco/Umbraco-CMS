// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for user password settings.
/// </summary>
[UmbracoOptions(Constants.Configuration.ConfigUserPassword)]
public class UserPasswordConfigurationSettings : IPasswordConfiguration
{
    /// <summary>
    ///     The default minimum required password length.
    /// </summary>
    internal const int StaticRequiredLength = 10;

    /// <summary>
    ///     The default value for requiring non-letter or digit characters.
    /// </summary>
    internal const bool StaticRequireNonLetterOrDigit = false;

    /// <summary>
    ///     The default value for requiring digit characters.
    /// </summary>
    internal const bool StaticRequireDigit = false;

    /// <summary>
    ///     The default value for requiring lowercase characters.
    /// </summary>
    internal const bool StaticRequireLowercase = false;

    /// <summary>
    ///     The default value for requiring uppercase characters.
    /// </summary>
    internal const bool StaticRequireUppercase = false;

    /// <summary>
    ///     The default maximum failed access attempts before lockout.
    /// </summary>
    internal const int StaticMaxFailedAccessAttemptsBeforeLockout = 5;

    /// <summary>
    ///     The default minimum response time for forgot password requests.
    /// </summary>
    internal const string StaticMinimumResponseTime = "0.00:00:02";

    /// <inheritdoc />
    [DefaultValue(StaticRequiredLength)]
    public int RequiredLength { get; set; } = StaticRequiredLength;

    /// <inheritdoc />
    [DefaultValue(StaticRequireNonLetterOrDigit)]
    public bool RequireNonLetterOrDigit { get; set; } = StaticRequireNonLetterOrDigit;

    /// <inheritdoc />
    [DefaultValue(StaticRequireDigit)]
    public bool RequireDigit { get; set; } = StaticRequireDigit;

    /// <inheritdoc />
    [DefaultValue(StaticRequireLowercase)]
    public bool RequireLowercase { get; set; } = StaticRequireLowercase;

    /// <inheritdoc />
    [DefaultValue(StaticRequireUppercase)]
    public bool RequireUppercase { get; set; } = StaticRequireUppercase;

    /// <inheritdoc />
    [DefaultValue(Constants.Security.AspNetCoreV3PasswordHashAlgorithmName)]
    public string HashAlgorithmType { get; set; } = Constants.Security.AspNetCoreV3PasswordHashAlgorithmName;

    /// <inheritdoc />
    [DefaultValue(StaticMaxFailedAccessAttemptsBeforeLockout)]
    public int MaxFailedAccessAttemptsBeforeLockout { get; set; } = StaticMaxFailedAccessAttemptsBeforeLockout;

    /// <summary>
    /// Gets or sets the minimum response time of the forgot password request.
    /// </summary>
    [DefaultValue(StaticMinimumResponseTime)]
    public TimeSpan MinimumResponseTime { get; set; } = TimeSpan.Parse(StaticMinimumResponseTime);
}
