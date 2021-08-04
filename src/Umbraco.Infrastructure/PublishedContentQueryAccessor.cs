using System;
using Microsoft.Extensions.DependencyInjection;

namespace Umbraco.Cms.Core
{
    public class PublishedContentQueryAccessor : IPublishedContentQueryAccessor
    {
        private readonly IServiceProvider _serviceProvider;

        public PublishedContentQueryAccessor(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public IPublishedContentQuery PublishedContentQuery => _serviceProvider.GetRequiredService<IPublishedContentQuery>();

    }
}
