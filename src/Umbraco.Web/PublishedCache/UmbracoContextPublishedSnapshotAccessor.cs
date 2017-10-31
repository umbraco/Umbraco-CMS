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

        public IPublishedShapshot PublishedSnapshot
        {
            get
            {
                var umbracoContext = _umbracoContextAccessor.UmbracoContext;
                if (umbracoContext == null) throw new Exception("The IUmbracoContextAccessor could not provide an UmbracoContext.");
                return umbracoContext.PublishedShapshot;
            }

            set
            {
                throw new NotSupportedException(); // not ok to set
            }
        }
    }
}
