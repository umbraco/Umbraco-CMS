// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Tests.Common.Builders.Extensions;

public static class ContentItemSaveBuilderExtensions
{
    public static ContentItemSaveBuilder WithContent(this ContentItemSaveBuilder builder, IContent content)
    {
        builder.WithId(content.Id);
        builder.WithContentTypeAlias(content.ContentType.Alias);

        if (content.CultureInfos.Count == 0)
        {
            var variantBuilder = builder.AddVariant();
            variantBuilder.WithName(content.Name);

            foreach (var contentProperty in content.Properties)
            {
                AddInvariantProperty(variantBuilder, contentProperty);
            }
        }
        else
        {
            foreach (var contentCultureInfos in content.CultureInfos)
            {
                var variantBuilder = builder.AddVariant();

                variantBuilder.WithName(contentCultureInfos.Name);
                variantBuilder.WithCultureInfo(contentCultureInfos.Culture);

                foreach (var contentProperty in content.Properties)
                {
                    AddInvariantProperty(variantBuilder, contentProperty);
                }
            }
        }

        return builder;
    }

    private static void AddInvariantProperty(ContentVariantSaveBuilder<ContentItemSaveBuilder> variantBuilder, IProperty contentProperty) =>
        variantBuilder
            .AddProperty()
            .WithId(contentProperty.Id)
            .WithAlias(contentProperty.Alias)
            .WithValue(contentProperty.GetValue())
            .Done();
}
