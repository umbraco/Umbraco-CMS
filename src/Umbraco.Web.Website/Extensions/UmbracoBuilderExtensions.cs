using Umbraco.Core.Builder;
using Umbraco.Web.Common.Builder;

namespace Umbraco.Extensions
{
    public static class UmbracoBuilderExtensions
    {
        public static IUmbracoBuilder WithAllWebsiteComponents(this IUmbracoBuilder builder)
        {
            builder
                .WithUmbracoWebsite();

            return builder;
        }

        public static IUmbracoBuilder WithUmbracoWebsite(this IUmbracoBuilder builder)
            => builder.AddWith(nameof(WithUmbracoWebsite), () => builder.Services.AddUmbracoWebsite());


    }
}
