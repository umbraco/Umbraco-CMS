using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Core.PropertyEditors.Validation;

public class NestedJsonPathValidationResults : ValidationResult, INestedValidationResults, IJsonPathValidationResult
{
    public string JsonPath { get; }

    public NestedJsonPathValidationResults(string jsonPath)
        : base(string.Empty)
        => JsonPath = jsonPath;

    public IList<ValidationResult> ValidationResults { get; } = [];
}
