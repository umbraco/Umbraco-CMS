using System;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Infrastructure;

namespace Umbraco.Cms.Core
{
    public class PublishedContentQueryAccessor : IPublishedContentQueryAccessor
    {
        private readonly IServiceProvider _serviceProvider;

        public PublishedContentQueryAccessor(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;
        public bool TryGetValue(out IPublishedContentQuery publishedContentQuery)
        {
            publishedContentQuery = _serviceProvider.GetRequiredService<IPublishedContentQuery>();

            return publishedContentQuery is not null;
        }

    }
}
