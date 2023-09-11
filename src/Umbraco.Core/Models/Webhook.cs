using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

[Serializable]
[DataContract(IsReference = true)]
public class Webhook : EntityBase
{
    private string _url;
    private WebhookEvent _webHookEvent;
    private Guid[] _entityKeys;

    public Webhook(string url, WebhookEvent webHookEvent, Guid[]? entityKeys = null)
    {
        _url = url;
        _webHookEvent = webHookEvent;
        _entityKeys = entityKeys ?? Array.Empty<Guid>();
    }

    [DataMember]
    public string Url
    {
        get => _url;
        set => SetPropertyValueAndDetectChanges(value, ref _url!, nameof(Url));
    }

    [DataMember]
    public WebhookEvent Event
    {
        get => _webHookEvent;
        set => SetPropertyValueAndDetectChanges(value, ref _webHookEvent!, nameof(Event));
    }

    [DataMember]
    public Guid[] EntityKeys
    {
        get => _entityKeys;
        set => SetPropertyValueAndDetectChanges(value, ref _entityKeys!, nameof(EntityKeys));
    }
}
