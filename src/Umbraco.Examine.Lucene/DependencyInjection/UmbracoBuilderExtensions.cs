using System.Collections.Generic;
using System.IO;
using System.Text;
using Examine;
using Examine.Lucene.Directories;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.DependencyInjection;

namespace Umbraco.Cms.Infrastructure.Examine.DependencyInjection
{
    public static class UmbracoBuilderExtensions
    {
        public static IUmbracoBuilder AddUmbracoIndexes(this IUmbracoBuilder umbracoBuilder)
        {
            IServiceCollection services = umbracoBuilder.Services;

            services.AddSingleton<IApplicationRoot, UmbracoApplicationRoot>();
            services.AddSingleton<ILockFactory, UmbracoLockFactory>();

            services
                .AddExamine()
                .AddExamineLuceneIndex<UmbracoContentIndex>(Constants.UmbracoIndexes.InternalIndexName)
                .AddExamineLuceneIndex<UmbracoContentIndex>(Constants.UmbracoIndexes.ExternalIndexName)
                .AddExamineLuceneIndex<UmbracoMemberIndex>(Constants.UmbracoIndexes.MembersIndexName)
                .ConfigureOptions<ConfigureIndexOptions>();

            return umbracoBuilder;
        }
    }
}
