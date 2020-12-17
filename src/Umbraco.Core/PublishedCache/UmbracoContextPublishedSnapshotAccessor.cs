using System;
namespace Umbraco.Web.PublishedCache
{
    // TODO: This is a mess. This is a circular reference:
    // IPublishedSnapshotAccessor -> PublishedSnapshotService -> UmbracoContext -> PublishedSnapshotService -> IPublishedSnapshotAccessor
    // Injecting IPublishedSnapshotAccessor into PublishedSnapshotService seems pretty strange
    // The underlying reason for this mess is because IPublishedContent is both a service and a model.
    // Until that is fixed, IPublishedContent will need to have a IPublishedSnapshotAccessor
    public class UmbracoContextPublishedSnapshotAccessor : IPublishedSnapshotAccessor
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        public UmbracoContextPublishedSnapshotAccessor(IUmbracoContextAccessor umbracoContextAccessor)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        public IPublishedSnapshot PublishedSnapshot
        {
            get
            {
                var umbracoContext = _umbracoContextAccessor.UmbracoContext;
                return umbracoContext?.PublishedSnapshot;
            }

            set => throw new NotSupportedException(); // not ok to set
        }
    }
}
