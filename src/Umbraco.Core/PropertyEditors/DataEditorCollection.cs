using System.Collections.Generic;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.PropertyEditors
{
    public class DataEditorCollection : BuilderCollectionBase<IDataEditor>
    {
        public DataEditorCollection(IEnumerable<IDataEditor> items)
            : base(items)
        { }
    }
}
