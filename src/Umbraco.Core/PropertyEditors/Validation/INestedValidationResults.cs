using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Core.PropertyEditors.Validation;

public interface INestedValidationResults
{
    IList<ValidationResult> ValidationResults { get; }
}
