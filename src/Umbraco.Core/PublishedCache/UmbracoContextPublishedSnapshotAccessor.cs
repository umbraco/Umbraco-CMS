using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Core.PublishedCache;

// TODO: This is a mess. This is a circular reference:
// IPublishedSnapshotAccessor -> PublishedSnapshotService -> UmbracoContext -> PublishedSnapshotService -> IPublishedSnapshotAccessor
// Injecting IPublishedSnapshotAccessor into PublishedSnapshotService seems pretty strange
// The underlying reason for this mess is because IPublishedContent is both a service and a model.
// Until that is fixed, IPublishedContent will need to have a IPublishedSnapshotAccessor
public class UmbracoContextPublishedSnapshotAccessor : IPublishedSnapshotAccessor
{
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;

    public UmbracoContextPublishedSnapshotAccessor(IUmbracoContextAccessor umbracoContextAccessor) =>
        _umbracoContextAccessor = umbracoContextAccessor;

    public IPublishedSnapshot? PublishedSnapshot
    {
        get
        {
            if (!_umbracoContextAccessor.TryGetUmbracoContext(out IUmbracoContext? umbracoContext))
            {
                return null;
            }

            return umbracoContext?.PublishedSnapshot;
        }

        set => throw new NotSupportedException(); // not ok to set
    }

    public bool TryGetPublishedSnapshot(out IPublishedSnapshot? publishedSnapshot)
    {
        if (!_umbracoContextAccessor.TryGetUmbracoContext(out IUmbracoContext? umbracoContext))
        {
            publishedSnapshot = null;
            return false;
        }

        publishedSnapshot = umbracoContext?.PublishedSnapshot;

        return publishedSnapshot is not null;
    }
}
