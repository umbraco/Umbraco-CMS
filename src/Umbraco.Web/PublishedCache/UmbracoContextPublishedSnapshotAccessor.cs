using System;

namespace Umbraco.Web.PublishedCache
{
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
