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
    internal const int StaticRequiredLength = 10;
    internal const bool StaticRequireNonLetterOrDigit = false;
    internal const bool StaticRequireDigit = false;
    internal const bool StaticRequireLowercase = false;
    internal const bool StaticRequireUppercase = false;
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
