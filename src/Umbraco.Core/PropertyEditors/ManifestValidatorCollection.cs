using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Composing;

namespace Umbraco.Core.PropertyEditors
{
    public class ManifestValidatorCollection : BuilderCollectionBase<ManifestValidator>
    {
        public ManifestValidatorCollection(IEnumerable<ManifestValidator> items)
            : base(items)
        { }

        public ManifestValidator this[string name] => this.FirstOrDefault(x => x.ValidationName.InvariantEquals(name));
    }
}
