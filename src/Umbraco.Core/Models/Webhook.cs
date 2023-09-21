using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models;

public class Webhook : EntityBase
{
    private string _url;
    private string[] _events;
    private Guid[] _entityKeys;
    private bool _enabled;

    // Custom comparer for enumerable guids
    private static readonly DelegateEqualityComparer<IEnumerable<Guid>> _guidEnumerableComparer =
        new(
            (enum1, enum2) => enum1.UnsortedSequenceEqual(enum2),
            enum1 => enum1.GetHashCode());

    // Custom comparer for enumerable webhook events
    private static readonly DelegateEqualityComparer<IEnumerable<string>> _webhookEventEnumerableComparer =
        new(
            (enum1, enum2) => enum1.UnsortedSequenceEqual(enum2),
            enum1 => enum1.GetHashCode());

    public Webhook(string url, bool? enabled = null, Guid[]? entityKeys = null, string[]? events = null)
    {
        _url = url;
        _events = events ?? Array.Empty<string>();
        _entityKeys = entityKeys ?? Array.Empty<Guid>();
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
        set => SetPropertyValueAndDetectChanges(value, ref _events!, nameof(Events), _webhookEventEnumerableComparer);
    }

    public Guid[] EntityKeys
    {
        get => _entityKeys;
        set => SetPropertyValueAndDetectChanges(value, ref _entityKeys!, nameof(EntityKeys), _guidEnumerableComparer);
    }

    public bool Enabled
    {
        get => _enabled;
        set => SetPropertyValueAndDetectChanges(value, ref _enabled, nameof(Enabled));
    }
}
