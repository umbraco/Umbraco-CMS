namespace Umbraco.Cms.Core.Models.ContentApi;

public interface IApiElement
{
    Guid Id { get; }

    string ContentType { get; }

    IDictionary<string, object?> Properties { get; }
}
