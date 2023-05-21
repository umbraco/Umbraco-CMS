// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Configuration;

/// <summary>
///     Password configuration
/// </summary>
public interface IPasswordConfiguration
{
    /// <summary>
    ///     Gets a value for the minimum required length for the password.
    /// </summary>
    int RequiredLength { get; }

    /// <summary>
    ///     Gets a value indicating whether at least one non-letter or digit is required for the password.
    /// </summary>
    bool RequireNonLetterOrDigit { get; }

    /// <summary>
    ///     Gets a value indicating whether at least one digit is required for the password.
    /// </summary>
    bool RequireDigit { get; }

    /// <summary>
    ///     Gets a value indicating whether at least one lower-case character is required for the password.
    /// </summary>
    bool RequireLowercase { get; }

    /// <summary>
    ///     Gets a value indicating whether at least one upper-case character is required for the password.
    /// </summary>
    bool RequireUppercase { get; }

    /// <summary>
    ///     Gets a value for the password hash algorithm type.
    /// </summary>
    string HashAlgorithmType { get; }

    /// <summary>
    ///     Gets a value for the maximum failed access attempts before lockout.
    /// </summary>
    /// <remarks>
    ///     TODO: This doesn't really belong here
    /// </remarks>
    int MaxFailedAccessAttemptsBeforeLockout { get; }
}
