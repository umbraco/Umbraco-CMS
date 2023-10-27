using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models;

public class Webhook
{
    public Webhook(string url, bool? enabled = null, Guid[]? entityKeys = null, string[]? events = null, Dictionary<string, string>? headers = null)
    {
        Url = url;
        Headers = headers ?? new Dictionary<string, string>();
        Events = events ?? Array.Empty<string>();
        EntityKeys = entityKeys ?? Array.Empty<Guid>();
        Enabled = enabled ?? false;
    }

    public int Id { get; set; }

    public Guid Key { get; set; }

    public string Url { get; set; }

    public string[] Events { get; set; }

    public Guid[] EntityKeys {get; set; }

    public bool Enabled { get; set; }

    public Dictionary<string, string> Headers { get; set; }
}
