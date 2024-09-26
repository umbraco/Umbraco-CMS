using Umbraco.Cms.Core.Models.ContentTypeEditing;

namespace Umbraco.Cms.Tests.Common.Builders;

public class MediaTypeEditingBuilder : ContentTypeEditingBaseBuilder<MediaTypeEditingBuilder, MediaTypeCreateModel, MediaTypePropertyTypeModel, MediaTypePropertyContainerModel>
{
    private Guid? _key;
    private Guid? _containerKey;

    public MediaTypeEditingBuilder WithKey(Guid key)
    {
        _key = key;
        return this;
    }

    public MediaTypeEditingBuilder WithContainerKey(Guid containerKey)
    {
        _containerKey = containerKey;
        return this;
    }

    public override MediaTypeCreateModel Build()
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
            .WithAllowAtRoot(true)
            .Build();
    }
}
