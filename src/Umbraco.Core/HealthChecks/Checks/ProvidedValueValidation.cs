// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.HealthChecks.Checks;

/// <summary>
///     Specifies the type of validation to apply to a provided value in a health check action.
/// </summary>
public enum ProvidedValueValidation
{
    /// <summary>
    ///     No validation is required for the provided value.
    /// </summary>
    None = 1,

    /// <summary>
    ///     The provided value must be a valid email address.
    /// </summary>
    Email = 2,

    /// <summary>
    ///     The provided value must match a specified regular expression.
    /// </summary>
    Regex = 3,
}
