using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Core.PropertyEditors.Validation;

/// <summary>
///     A collection of <see cref="ValidationResult" /> for a property type within a complex editor represented by an
///     Element Type
/// </summary>
/// <remarks>
///     For a more indepth explanation of how server side validation works with the angular app, see this GitHub PR:
///     https://github.com/umbraco/Umbraco-CMS/pull/8339
/// </remarks>
public class ComplexEditorPropertyTypeValidationResult : ValidationResult
{
    private readonly List<ValidationResult> _validationResults = new();

    public ComplexEditorPropertyTypeValidationResult(string propertyTypeAlias)
        : base(string.Empty) =>
        PropertyTypeAlias = propertyTypeAlias;

    public IReadOnlyList<ValidationResult> ValidationResults => _validationResults;

    public string PropertyTypeAlias { get; }

    public void AddValidationResult(ValidationResult validationResult)
    {
        if (validationResult is ComplexEditorValidationResult &&
            _validationResults.Any(x => x is ComplexEditorValidationResult))
        {
            throw new InvalidOperationException($"Cannot add more than one {typeof(ComplexEditorValidationResult)}");
        }

        _validationResults.Add(validationResult);
    }
}
