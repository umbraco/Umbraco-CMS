using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core;

[Obsolete("This will be removed in Umbraco 15.")]
public class UmbracoApiControllerTypeCollection : BuilderCollectionBase<Type>
{
    public UmbracoApiControllerTypeCollection(Func<IEnumerable<Type>> items)
        : base(items)
    {
    }
}
