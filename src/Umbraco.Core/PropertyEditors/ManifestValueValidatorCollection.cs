using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Composing;

namespace Umbraco.Core.PropertyEditors
{
    public class ManifestValueValidatorCollection : BuilderCollectionBase<IManifestValueValidator>
    {
        public ManifestValueValidatorCollection(IEnumerable<IManifestValueValidator> items)
            : base(items)
        { }

        public IManifestValueValidator Create(string name)
        {
            var v = this.FirstOrDefault(x => x.ValidationName.InvariantEquals(name));
            if (v == null)
                throw new InvalidOperationException($"Could not find a validator named \"{name}\".");

            // TODO: what is this exactly?
            // we cannot return this instance, need to clone it?
            return (IManifestValueValidator) Activator.CreateInstance(v.GetType()); // ouch
        }
    }
}
