namespace Umbraco.Cms.Core.Models;

public class Webhook
{
    public Webhook(string url, bool? enabled = null, Guid[]? entityKeys = null, string[]? events = null, IDictionary<string, string>? headers = null)
    {
        Url = url;
        Headers = headers ?? new Dictionary<string, string>();
        Events = events ?? Array.Empty<string>();
        ContentTypeKeys = entityKeys ?? Array.Empty<Guid>();
        Enabled = enabled ?? false;
    }

    public int Id { get; set; }

    public Guid Key { get; set; }

    public string Url { get; set; }

    public string[] Events { get; set; }

    public Guid[] ContentTypeKeys {get; set; }

    public bool Enabled { get; set; }

    public IDictionary<string, string> Headers { get; set; }
}
