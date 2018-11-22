using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Umbraco.Web.Editors
{
    internal abstract class EditorValidator<T> : IEditorValidator
    {
        public Type ModelType
        {
            get { return typeof (T); }
        }

        protected abstract IEnumerable<ValidationResult> PerformValidate(T model);

        public IEnumerable<ValidationResult> Validate(object model)
        {
            return PerformValidate((T) model);
        }
    }
}