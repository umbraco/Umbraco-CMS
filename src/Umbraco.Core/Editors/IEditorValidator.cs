using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Editors;

// note - about IEditorValidator
//
// interface: IEditorValidator
// base class: EditorValidator<T>
// static validation: EditorValidator.Validate()
// composition: via EditorValidationCollection and builder
//              initialized with all IEditorValidator instances
//
// validation is used exclusively in ContentTypeControllerBase
// currently the only implementations are for Models Builder.

/// <summary>
///     Provides a general object validator.
/// </summary>
public interface IEditorValidator : IDiscoverable
{
    /// <summary>
    ///     Gets the object type validated by this validator.
    /// </summary>
    Type ModelType { get; }

    /// <summary>
    ///     Validates an object.
    /// </summary>
    IEnumerable<ValidationResult> Validate(object model);
}
