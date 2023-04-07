namespace Umbraco.Cms.Core.Models.PublishedContent;

/// <inheritdoc />
public class PublishedModelProperty<T> : IPublishedModelProperty<T>
{
    /// <inheritdoc/>
    public string? Alias { get; set; }
    /// <inheritdoc/>
    public T? Value { get; set; }

}
