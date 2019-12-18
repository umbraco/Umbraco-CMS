using System.Collections.Generic;
using Umbraco.Core.Composing;

namespace Umbraco.Core.PropertyEditors
{
    public class DataEditorCollection : BuilderCollectionBase<IDataEditor>
    {
        public DataEditorCollection(IEnumerable<IDataEditor> items)
            : base(items)
        { }
    }
}
