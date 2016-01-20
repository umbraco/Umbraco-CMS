using System;

namespace Umbraco.Web.Editors
{
    internal interface IEditorValidator
    {
        Type ModelType { get; }
        void Validate(object model, EditorValidationErrors editorValidations);
    }
}