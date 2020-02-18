using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Composing;

namespace Umbraco.Core.PropertyEditors
{
    public class DataEditorWithMediaPathCollection : BuilderCollectionBase<IDataEditorWithMediaPath>
    {
        public DataEditorWithMediaPathCollection(IEnumerable<IDataEditorWithMediaPath> items) : base(items)
        {
        }

        public bool TryGet(string alias, out IDataEditorWithMediaPath editor)
        {
            editor = this.FirstOrDefault(x => x.Alias == alias);
            return editor != null;
        }


    }
}
