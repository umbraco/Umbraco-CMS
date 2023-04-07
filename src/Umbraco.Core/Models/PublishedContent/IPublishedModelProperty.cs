namespace Umbraco.Cms.Core.Models.PublishedContent;

public interface IPublishedModelProperty<T>
{
    public string? Alias { get; set; }
    public T? Value { get; set; }
}
