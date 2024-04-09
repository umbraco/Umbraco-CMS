using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Mapping.DataType;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class DataTypeBuilderExtensions
{
    internal static IUmbracoBuilder AddDataTypes(this IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<IDataTypeReferencePresentationFactory, DataTypeReferencePresentationFactory>();
        builder.Services.AddTransient<IDataTypePresentationFactory, DataTypePresentationFactory>();

        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>().Add<DataTypeViewModelMapDefinition>();

        return builder;
    }
}
