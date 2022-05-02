using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Web.Website.Collections;

public class SurfaceControllerTypeCollection : BuilderCollectionBase<Type>
{
    public SurfaceControllerTypeCollection(Func<IEnumerable<Type>> items)
        : base(items)
    {
    }
}
