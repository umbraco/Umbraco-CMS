using System.Collections.Generic;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Editors
{
    public class EditorValidatorCollection : BuilderCollectionBase<IEditorValidator>
    {
        public EditorValidatorCollection(IEnumerable<IEditorValidator> items)
            : base(items)
        { }
    }
}
