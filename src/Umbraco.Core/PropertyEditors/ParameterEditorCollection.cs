using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Composing;

namespace Umbraco.Core.PropertyEditors
{
    public class ParameterEditorCollection : BuilderCollectionBase<IDataEditor>
    {
        public ParameterEditorCollection(IEnumerable<IDataEditor> items)
            : base(items)
        { }

        // note: virtual so it can be mocked
        public virtual IDataEditor this[string alias]
            => this.SingleOrDefault(x => x.Alias == alias);

        public virtual bool TryGet(string alias, out IDataEditor editor)
        {
            editor = this.FirstOrDefault(x => x.Alias == alias);
            return editor != null;
        }
    }
}
