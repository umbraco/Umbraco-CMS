using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Api.Management.Mapping.Template;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class TemplateBuilderExtensions
{
    internal static IUmbracoBuilder AddTemplates(this IUmbracoBuilder builder)
    {
        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>().Add<TemplateViewModelMapDefinition>();
        builder.Services.AddTransient<ITemplatePresentationFactory, TemplatePresentationFactory>();

        return builder;
    }
}
