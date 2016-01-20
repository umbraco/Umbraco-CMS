using System;

namespace Umbraco.Web.Editors
{
    internal abstract class EditorValidator<T> : IEditorValidator
    {
        public Type ModelType
        {
            get { return typeof (T); }
        }

        public abstract void Validate(object model, EditorValidationErrors editorValidations);
    }
}