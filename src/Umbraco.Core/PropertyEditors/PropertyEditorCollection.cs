using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.DependencyInjection;

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
    }
}
