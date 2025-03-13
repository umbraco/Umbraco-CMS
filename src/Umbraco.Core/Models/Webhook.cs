using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models;

public class Webhook : EntityBase, IWebhook
{
    // Custom comparers for enumerable
    private static readonly DelegateEqualityComparer<IEnumerable<Guid>>
        ContentTypeKeysComparer =
            new(
                (enumerable, translations) => enumerable.UnsortedSequenceEqual(translations),
                enumerable => enumerable.GetHashCode());

    private static readonly DelegateEqualityComparer<IEnumerable<string>>
        EventsComparer =
            new(
                (enumerable, translations) => enumerable.UnsortedSequenceEqual(translations),
                enumerable => enumerable.GetHashCode());

    private static readonly DelegateEqualityComparer<IDictionary<string, string>>
        HeadersComparer =
            new(
                (enumerable, translations) => enumerable.UnsortedSequenceEqual(translations),
                enumerable => enumerable.GetHashCode());

    private string _url;
    private string[] _events;
    private Guid[] _contentTypeKeys;
    private bool _enabled;
    private IDictionary<string, string> _headers;

    public Webhook(string url, bool? enabled = null, Guid[]? entityKeys = null, string[]? events = null, IDictionary<string, string>? headers = null)
    {
        _url = url;
        _headers = headers ?? new Dictionary<string, string>();
        _events = events ?? Array.Empty<string>();
        _contentTypeKeys = entityKeys ?? Array.Empty<Guid>();
        _enabled = enabled ?? false;
    }

    public string Url
    {
        get => _url;
        set => SetPropertyValueAndDetectChanges(value, ref _url!, nameof(Url));
    }

    public string[] Events
    {
        get => _events;
        set => SetPropertyValueAndDetectChanges(value, ref _events!, nameof(Events), EventsComparer);
    }

    public Guid[] ContentTypeKeys
    {
        get => _contentTypeKeys;
        set => SetPropertyValueAndDetectChanges(value, ref _contentTypeKeys!, nameof(ContentTypeKeys), ContentTypeKeysComparer);
    }

    public bool Enabled
    {
        get => _enabled;
        set => SetPropertyValueAndDetectChanges(value, ref _enabled, nameof(Enabled));
    }

    public IDictionary<string, string> Headers
    {
        get => _headers;
        set => SetPropertyValueAndDetectChanges(value, ref _headers!, nameof(Headers), HeadersComparer);
    }
}
