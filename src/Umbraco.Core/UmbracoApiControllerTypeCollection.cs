using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core;

public class UmbracoApiControllerTypeCollection : BuilderCollectionBase<Type>
{
    public UmbracoApiControllerTypeCollection(Func<IEnumerable<Type>> items)
        : base(items)
    {
    }
}
