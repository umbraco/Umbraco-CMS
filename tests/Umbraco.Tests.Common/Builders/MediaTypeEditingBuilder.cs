using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders;

public class MediaTypeEditingBuilder : ContentTypeEditingBaseBuilder<MediaTypeEditingBuilder, MediaTypeCreateModel,
    MediaTypePropertyTypeModel, MediaTypePropertyContainerModel>, IWithKeyBuilder, IWithContainerKeyBuilder
{
    private Guid? _key;
    private Guid? _containerKey;

    Guid? IWithKeyBuilder.Key
    {
        get => _key;
        set => _key = value;
    }

    Guid? IWithContainerKeyBuilder.ContainerKey
    {
        get => _containerKey;
        set => _containerKey = value;
    }

    protected override MediaTypeCreateModel Build()
    {
        _model.Key = _key ?? Guid.NewGuid();
        _model.ContainerKey = _containerKey;
        base.Build();
        return _model;
    }

    public static MediaTypeCreateModel CreateBasicMediaType(string alias = "umbImage", string name = "Image")
    {
        var builder = new MediaTypeEditingBuilder();
        return (MediaTypeCreateModel)builder
            .WithAlias(alias)
            .WithName(name)
            .WithAllowAsRoot(true)
            .Build();
    }

    public static MediaTypeCreateModel CreateBasicFolderMediaType(string alias = "basicFolder", string name = "BasicFolder")
    {
        var builder = new MediaTypeEditingBuilder();
        return (MediaTypeCreateModel)builder
            .WithAlias(alias)
            .WithName(name)
            .WithIcon("icon-folder")
            .WithAllowAsRoot(true)
            .Build();
    }

    public static MediaTypeCreateModel CreateMediaTypeWithOneProperty(string alias = "testMediaType", string name = "TestMediaType", string propertyAlias = "testProperty", string propertyName = "TestProperty")
    {
        var containerKey = Guid.NewGuid();
        var builder = new MediaTypeEditingBuilder();
        return (MediaTypeCreateModel)builder
            .WithAlias(alias)
            .WithName(name)
            .WithAllowAsRoot(true)
            .AddPropertyGroup()
                .WithName("TestGroup")
                .WithKey(containerKey)
                .Done()
            .AddPropertyType()
                .WithAlias(propertyAlias)
                .WithName(propertyName)
                .WithDataTypeKey(Constants.DataTypes.Guids.TextstringGuid)
                .WithContainerKey(containerKey)
                .Done()
            .Build();
    }
}
