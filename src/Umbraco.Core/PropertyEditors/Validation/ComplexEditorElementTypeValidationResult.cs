using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Core.PropertyEditors.Validation;

/// <summary>
///     A collection of <see cref="ComplexEditorPropertyTypeValidationResult" /> for an element type within complex editor
///     represented by an Element Type
/// </summary>
/// <remarks>
///     For a more indepth explanation of how server side validation works with the angular app, see this GitHub PR:
///     https://github.com/umbraco/Umbraco-CMS/pull/8339
/// </remarks>
public class ComplexEditorElementTypeValidationResult : ValidationResult
{
    public ComplexEditorElementTypeValidationResult(string elementTypeAlias, Guid blockId)
        : base(string.Empty)
    {
        ElementTypeAlias = elementTypeAlias;
        BlockId = blockId;
    }

    public IList<ComplexEditorPropertyTypeValidationResult> ValidationResults { get; } =
        new List<ComplexEditorPropertyTypeValidationResult>();

    /// <summary>
    ///     The element type alias of the validation result
    /// </summary>
    /// <remarks>
    ///     This is useful for debugging purposes but it's not actively used in the angular app
    /// </remarks>
    public string ElementTypeAlias { get; }

    /// <summary>
    ///     The Block ID of the validation result
    /// </summary>
    /// <remarks>
    ///     This is the GUID id of the content item based on the element type
    /// </remarks>
    public Guid BlockId { get; }
}
