namespace Umbraco.Cms.Core.Models.ContentApi;

public interface IApiElement
{
    Guid Key { get; }

    string ContentType { get; }

    IDictionary<string, object?> Properties { get; }
}
