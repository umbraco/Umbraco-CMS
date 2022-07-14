using Umbraco.Cms.Core.Composing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

public class ManifestValueValidatorCollection : BuilderCollectionBase<IManifestValueValidator>
{
    public ManifestValueValidatorCollection(Func<IEnumerable<IManifestValueValidator>> items)
        : base(items)
    {
    }

    public IManifestValueValidator? Create(string name)
    {
        IManifestValueValidator v = GetByName(name);

        // TODO: what is this exactly?
        // we cannot return this instance, need to clone it?
        return (IManifestValueValidator?)Activator.CreateInstance(v.GetType()); // ouch
    }

    public IManifestValueValidator GetByName(string name)
    {
        IManifestValueValidator? v = this.FirstOrDefault(x => x.ValidationName.InvariantEquals(name));
        if (v == null)
        {
            throw new InvalidOperationException($"Could not find a validator named \"{name}\".");
        }

        return v;
    }
}
