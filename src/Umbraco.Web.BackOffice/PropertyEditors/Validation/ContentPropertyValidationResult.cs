using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Umbraco.Cms.Core.PropertyEditors.Validation;

namespace Umbraco.Cms.Web.BackOffice.PropertyEditors.Validation;

/// <summary>
///     Custom <see cref="ValidationResult" /> for content properties
/// </summary>
/// <remarks>
///     This clones the original result and then ensures the nested result if it's the correct type.
///     For a more indepth explanation of how server side validation works with the angular app, see this GitHub PR:
///     https://github.com/umbraco/Umbraco-CMS/pull/8339
/// </remarks>
public class ContentPropertyValidationResult : ValidationResult
{
    private readonly string _culture;
    private readonly string _segment;

    public ContentPropertyValidationResult(ValidationResult nested, string culture, string segment)
        : base(nested.ErrorMessage, nested.MemberNames)
    {
        ComplexEditorResults = nested as ComplexEditorValidationResult;
        _culture = culture;
        _segment = segment;
    }

    /// <summary>
    ///     Nested validation results for the content property
    /// </summary>
    /// <remarks>
    ///     There can be nested results for complex editors that contain other editors
    /// </remarks>
    public ComplexEditorValidationResult? ComplexEditorResults { get; }

    /// <summary>
    ///     Return the <see cref="ValidationResult.ErrorMessage" /> if <see cref="ComplexEditorResults" /> is null, else the
    ///     serialized
    ///     complex validation results
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        if (ComplexEditorResults == null)
        {
            return base.ToString();
        }

        var json = JsonConvert.SerializeObject(this, new ValidationResultConverter(_culture, _segment));
        return json;
    }
}
