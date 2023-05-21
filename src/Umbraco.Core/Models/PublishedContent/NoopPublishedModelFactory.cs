using System.Collections;

namespace Umbraco.Cms.Core.Models.PublishedContent;

/// <inheritdoc />
/// <summary>Represents a no-operation factory.</summary>
public class NoopPublishedModelFactory : IPublishedModelFactory
{
    /// <inheritdoc />
    public IPublishedElement CreateModel(IPublishedElement element) => element;

    /// <inheritdoc />
    public IList CreateModelList(string? alias) => new List<IPublishedElement>();

    /// <inheritdoc />
    public Type GetModelType(string? alias) => typeof(IPublishedElement);

    /// <inheritdoc />
    public Type MapModelType(Type type) => typeof(IPublishedElement);
}
