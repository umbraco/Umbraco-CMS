using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Core.PropertyEditors.Validation;

public class NestedValidationResults : ValidationResult, INestedValidationResults
{
    public NestedValidationResults()
        : base(string.Empty)
    {
    }

    public IList<ValidationResult> ValidationResults { get; } = [];
}
