using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;
using Umbraco.Cms.Tests.Common.Builders.Interfaces.ContentCreateModel;

namespace Umbraco.Cms.Tests.Common.Builders.Extensions;

public static class ContentEditingBuilderExtensions
{
    public static T WithInvariantName<T>(this T Builder, string invariantName)
        where T : IWithInvariantNameBuilder
    {
        Builder.InvariantName = invariantName;
        return Builder;
    }

    public static T WithInvariantProperties<T>(this T Builder, IEnumerable<PropertyValueModel> invariantProperties)
        where T : IWithInvariantPropertiesBuilder
    {
        Builder.InvariantProperties = invariantProperties;
        return Builder;
    }

    public static T WithVariants<T>(this T Builder, IEnumerable<VariantModel> variants)
        where T : IWithVariantsBuilder
    {
        Builder.Variants = variants;
        return Builder;
    }

    public static T WithKey<T>(this T Builder, Guid? key)
        where T : IWithKeyBuilder
    {
        Builder.Key = key;
        return Builder;
    }

    public static T WithContentTypeKey<T>(this T Builder, Guid contentTypeKey)
        where T : IWithContentTypeKeyBuilder
    {
        Builder.ContentTypeKey = contentTypeKey;
        return Builder;
    }

    public static T WithParentKey<T>(this T Builder, Guid? parentKey)
        where T : IWithParentKeyBuilder
    {
        Builder.ParentKey = parentKey;
        return Builder;
    }


    public static T WithTemplateKey<T>(this T Builder, Guid? templateKey)
        where T : IWithTemplateKeyBuilder
    {
        Builder.TemplateKey = templateKey;
        return Builder;
    }
}
