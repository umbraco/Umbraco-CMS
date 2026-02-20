using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a webhook configuration that triggers HTTP requests on specified events.
/// </summary>
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

    private string? _name;
    private string? _description;
    private string _url;
    private string[] _events;
    private Guid[] _contentTypeKeys;
    private bool _enabled;
    private IDictionary<string, string> _headers;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Webhook" /> class.
    /// </summary>
    /// <param name="url">The URL to send webhook requests to.</param>
    /// <param name="enabled">Indicates whether the webhook is enabled. Defaults to <c>false</c>.</param>
    /// <param name="entityKeys">The content type keys that trigger this webhook.</param>
    /// <param name="events">The event aliases that trigger this webhook.</param>
    /// <param name="headers">Custom HTTP headers to include in webhook requests.</param>
    public Webhook(string url, bool? enabled = null, Guid[]? entityKeys = null, string[]? events = null, IDictionary<string, string>? headers = null)
    {
        _url = url;
        _headers = headers ?? new Dictionary<string, string>();
        _events = events ?? Array.Empty<string>();
        _contentTypeKeys = entityKeys ?? Array.Empty<Guid>();
        _enabled = enabled ?? false;
    }

    /// <summary>
    ///     Gets or sets the display name of the webhook.
    /// </summary>
    public string? Name
    {
        get => _name;
        set => SetPropertyValueAndDetectChanges(value, ref _name!, nameof(Name));
    }

    /// <summary>
    ///     Gets or sets the description of the webhook.
    /// </summary>
    public string? Description
    {
        get => _description;
        set => SetPropertyValueAndDetectChanges(value, ref _description!, nameof(Description));
    }

    /// <summary>
    ///     Gets or sets the URL to send webhook requests to.
    /// </summary>
    public string Url
    {
        get => _url;
        set => SetPropertyValueAndDetectChanges(value, ref _url!, nameof(Url));
    }

    /// <summary>
    ///     Gets or sets the event aliases that trigger this webhook.
    /// </summary>
    public string[] Events
    {
        get => _events;
        set => SetPropertyValueAndDetectChanges(value, ref _events!, nameof(Events), EventsComparer);
    }

    /// <summary>
    ///     Gets or sets the content type keys that filter which content triggers this webhook.
    /// </summary>
    /// <remarks>
    ///     If empty, the webhook triggers for all content types.
    /// </remarks>
    public Guid[] ContentTypeKeys
    {
        get => _contentTypeKeys;
        set => SetPropertyValueAndDetectChanges(value, ref _contentTypeKeys!, nameof(ContentTypeKeys), ContentTypeKeysComparer);
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the webhook is enabled.
    /// </summary>
    public bool Enabled
    {
        get => _enabled;
        set => SetPropertyValueAndDetectChanges(value, ref _enabled, nameof(Enabled));
    }

    /// <summary>
    ///     Gets or sets the custom HTTP headers to include in webhook requests.
    /// </summary>
    public IDictionary<string, string> Headers
    {
        get => _headers;
        set => SetPropertyValueAndDetectChanges(value, ref _headers!, nameof(Headers), HeadersComparer);
    }
}
