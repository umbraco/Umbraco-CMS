using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Core.Models.Membership;

/// <summary>
///     Represents a base class for operation results that may contain an error message.
/// </summary>
public abstract class ErrorMessageResult
{
    /// <summary>
    ///     Gets or sets the validation error result, if any.
    /// </summary>
    public ValidationResult? Error { get; set; }
}
