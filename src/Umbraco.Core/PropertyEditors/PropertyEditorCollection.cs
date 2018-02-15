using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Composing;

namespace Umbraco.Core.PropertyEditors
{
    public class PropertyEditorCollection : BuilderCollectionBase<IConfiguredDataEditor>
    {
        public PropertyEditorCollection(IEnumerable<IConfiguredDataEditor> items)
            : base(items)
        { }

        // note: virtual so it can be mocked
        public virtual IConfiguredDataEditor this[string alias]
            => this.SingleOrDefault(x => x.Alias == alias);

        public virtual bool TryGet(string alias, out IConfiguredDataEditor editor)
        {
            editor = this.FirstOrDefault(x => x.Alias == alias);
            return editor != null;
        }
    }
}
