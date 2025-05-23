using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

public interface IWebhook : IEntity
{
    // TODO (V16): Remove the default implementations from this interface.
    string? Name
    {
        get { return null; }
        set { }
    }

    string? Description
    {
        get { return null; }
        set { }
    }

    string Url { get; set; }

    string[] Events { get; set; }

    Guid[] ContentTypeKeys {get; set; }

    bool Enabled { get; set; }

    IDictionary<string, string> Headers { get; set; }
}
