using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Core.PropertyEditors.Validation;

/// <summary>
///     A collection of <see cref="ComplexEditorElementTypeValidationResult" /> for a complex editor represented by an
///     Element Type
/// </summary>
/// <remarks>
///     For example, each <see cref="ComplexEditorValidationResult" /> represents validation results for a row in Nested
///     Content.
///     For a more indepth explanation of how server side validation works with the angular app, see this GitHub PR:
///     https://github.com/umbraco/Umbraco-CMS/pull/8339
/// </remarks>
public class ComplexEditorValidationResult : ValidationResult
{
    public ComplexEditorValidationResult()
        : base(string.Empty)
    {
    }

    public IList<ComplexEditorElementTypeValidationResult> ValidationResults { get; } =
        new List<ComplexEditorElementTypeValidationResult>();
}
