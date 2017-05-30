using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Composing;

namespace Umbraco.Core.PropertyEditors
{
    internal class ValidatorCollection : BuilderCollectionBase<ManifestValueValidator>
    {
        public ValidatorCollection(IEnumerable<ManifestValueValidator> items) 
            : base(items)
        { }

        public ManifestValueValidator this[string name] => this.FirstOrDefault(x => x.TypeName.InvariantEquals(name));
    }
}
