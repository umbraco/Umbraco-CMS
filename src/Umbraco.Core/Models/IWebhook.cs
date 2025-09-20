using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

public interface IWebhook : IEntity
{
    string Url { get; set; }

    string[] Events { get; set; }

    Guid[] ContentTypeKeys {get; set; }

    bool Enabled { get; set; }

    IDictionary<string, string> Headers { get; set; }
}
