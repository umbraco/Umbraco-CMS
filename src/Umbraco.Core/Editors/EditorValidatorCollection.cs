using System.Collections.Generic;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Editors
{
    public class EditorValidatorCollection : BuilderCollectionBase<IEditorValidator>
    {
        public EditorValidatorCollection(IEnumerable<IEditorValidator> items)
            : base(items)
        { }
    }
}
