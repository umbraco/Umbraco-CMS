using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders;

public class MediaTypeEditingBuilder : ContentTypeEditingBuilderBase<MediaTypeEditingBuilder, MediaTypeCreateModel,
        MediaTypePropertyTypeModel, MediaTypePropertyContainerModel>,
    IBuildPropertyTypes
{
    private Guid? _key;
    private Guid? _containerKey;

    // Constructor
    public MediaTypeEditingBuilder()
    {
    }

    public override MediaTypeCreateModel Build()
    {
        var mediaType = new MediaTypeCreateModel { Key = _key ?? Guid.NewGuid(), ContainerKey = _containerKey, };

        return mediaType;
    }

    public static MediaTypeCreateModel CreateBasicMediaType(string alias = "umbImage", string name = "Image")
    {
        var builder = new MediaTypeEditingBuilder();
        return builder
            .WithAlias(alias)
            .WithName(name)
            .Build();
    }

}

