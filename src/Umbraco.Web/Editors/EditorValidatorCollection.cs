using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Editors
{
    internal class EditorValidatorCollection : BuilderCollectionBase<IEditorValidator>
    {
        public EditorValidatorCollection(IEnumerable<IEditorValidator> items) 
            : base(items)
        { }

        public IEnumerable<ValidationResult> Validate(object model)
        {
            var modelType = model.GetType();
            return this
                .Where(x => x.ModelType == modelType)
                .WhereNotNull()
                .SelectMany(x => x.Validate(model));
        }
    }
}
