// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Core.Configuration.Models.Validation;

/// <summary>
///     Provides a base class for configuration models that can be validated based on data annotations.
/// </summary>
public abstract class ValidatableEntryBase
{
    /// <summary>
    ///     Validates the configuration entry using data annotations.
    /// </summary>
    /// <returns><c>true</c> if the entry is valid; otherwise, <c>false</c>.</returns>
    internal virtual bool IsValid()
    {
        var ctx = new ValidationContext(this);
        var results = new List<ValidationResult>();
        return Validator.TryValidateObject(this, ctx, results, true);
    }
}
