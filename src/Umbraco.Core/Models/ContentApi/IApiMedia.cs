namespace Umbraco.Cms.Core.Models.ContentApi;

public interface IApiMedia
{
    public Guid Id { get; }

    public string Name { get; }

    public string MediaType { get; }

    public string Url { get; }

    public string? Extension { get; }

    public int? Width { get; }

    public int? Height { get; }

    public IDictionary<string, object?> Properties { get; }
}
