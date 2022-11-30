namespace Umbraco.Cms.Core.Headless;

public interface IHeadlessElement
{
    Guid Key { get; }

    string ContentType { get; }

    IDictionary<string, object?> Properties { get; }
}
