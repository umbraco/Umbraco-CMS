﻿using Umbraco.Web.Common.Builder;

namespace Umbraco.Extensions
{
    public static class UmbracoBuilderExtensions
    {
        public static void BuildWithAllBackOfficeComponents(this IUmbracoBuilder builder)
        {
            builder
                .WithConfiguration()
                .WithCore()
                .WithWebComponents()
                .WithRuntimeMinifier()
                .WithBackOffice()
                .WithBackOfficeIdentity()
                .WithMiniProfiler()
                .WithMvcAndRazor()
                .WithWebServer()
                .WithPreview()
                .Build();
        }

        public static IUmbracoBuilder WithBackOffice(this IUmbracoBuilder builder)
            => builder.AddWith(nameof(WithBackOffice), () => builder.Services.AddUmbracoBackOffice());

        public static IUmbracoBuilder WithBackOfficeIdentity(this IUmbracoBuilder builder)
            => builder.AddWith(nameof(WithBackOfficeIdentity), () => builder.Services.AddUmbracoBackOfficeIdentity());

        public static IUmbracoBuilder WithPreview(this IUmbracoBuilder builder)
            => builder.AddWith(nameof(WithPreview), () => builder.Services.AddUmbracoPreview());
    }
}
