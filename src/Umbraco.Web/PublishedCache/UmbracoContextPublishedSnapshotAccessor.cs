using System;

namespace Umbraco.Web.PublishedCache
{
    public class UmbracoContextPublishedSnapshotAccessor : IPublishedSnapshotAccessor
    {
        private readonly IUmbracoContextFactory _umbracoContextFactory;

        public UmbracoContextPublishedSnapshotAccessor(IUmbracoContextFactory umbracoContextFactory)
        {
            _umbracoContextFactory = umbracoContextFactory;
        }

        public IPublishedSnapshot PublishedSnapshot
        {
            get
            {
                using (var context = _umbracoContextFactory.EnsureUmbracoContext())
                {
                    return context.UmbracoContext?.PublishedSnapshot;
                }
            }

            set => throw new NotSupportedException(); // not ok to set
        }
    }
}
