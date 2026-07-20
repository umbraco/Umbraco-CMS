// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for member password settings.
/// </summary>
[UmbracoOptions(Constants.Configuration.ConfigMemberPassword)]
public class MemberPasswordConfigurationSettings : IPasswordConfiguration
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
}
