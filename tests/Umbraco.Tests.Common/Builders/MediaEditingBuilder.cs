using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Common.Builders;

public class MediaEditingBuilder : ContentEditingBaseBuilder<MediaCreateModel>
{
    public static MediaCreateModel CreateBasicMedia(Guid mediaTypeKey, Guid? key) =>
        new MediaEditingBuilder()
            .WithKey(key)
            .WithContentTypeKey(mediaTypeKey)
            .WithInvariantName("Media")
            .Build();

    public static MediaCreateModel CreateSimpleMedia(Guid mediaTypeKey, string name, Guid? parentKey) =>
        new MediaEditingBuilder()
            .WithContentTypeKey(mediaTypeKey)
            .WithInvariantName(name)
            .WithParentKey(parentKey)
            .Build();

    public static MediaCreateModel CreateMediaWithAProperty(Guid mediaTypeKey, string name, Guid? parentKey, string propertyAlias = "testProperty", string propertyValue = "TestValue") =>
        new MediaEditingBuilder()
            .WithContentTypeKey(mediaTypeKey)
            .WithInvariantName(name)
            .WithParentKey(parentKey)
            .AddInvariantProperty()
                .WithAlias(propertyAlias)
                .WithValue(propertyValue)
                .Done()
            .Build();
}
