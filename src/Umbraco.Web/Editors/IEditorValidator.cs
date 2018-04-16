using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Editors
{
    // note - about IEditorValidator
    //
    // interface: IEditorValidator
    // base class: EditorValidator<T>
    // static validation: EditorValidator.Validate()
    // composition: via EditorValidationCollection and builder
    //              initialized with all IEditorValidator instances
    //
    // validation is used exclusively in ContentTypeControllerBase
    // the whole thing is internal at the moment, never released
    // and, there are no IEditorValidator implementation in Core
    // so... this all mechanism is basically useless

    /// <summary>
    /// Provides a general object validator.
    /// </summary>
    internal interface IEditorValidator : IDiscoverable
    {
        /// <summary>
        /// Gets the object type validated by this validator.
        /// </summary>
        Type ModelType { get; }

        /// <summary>
        /// Validates an object.
        /// </summary>
        IEnumerable<ValidationResult> Validate(object model);
    }
}
