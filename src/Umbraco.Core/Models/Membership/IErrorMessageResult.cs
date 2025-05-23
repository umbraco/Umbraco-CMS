using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Core.Models.Membership;

public abstract class ErrorMessageResult
{
    public ValidationResult? Error { get; set; }
}
