using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.DI;

namespace Umbraco.Core.PropertyEditors
{
    public class PropertyEditorCollection : BuilderCollectionBase<PropertyEditor>
    {
        public PropertyEditorCollection(IEnumerable<PropertyEditor> items)
            : base(items)
        { }

        // note: virtual so it can be mocked
        public virtual PropertyEditor this[string alias]
            => this.SingleOrDefault(x => x.Alias == alias);

        public virtual bool TryGet(string alias, out PropertyEditor editor)
        {
            editor = this.FirstOrDefault(x => x.Alias == alias);
            return editor != null;
        }
    }
}
