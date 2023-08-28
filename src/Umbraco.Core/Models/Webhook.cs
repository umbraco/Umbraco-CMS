using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

[Serializable]
[DataContract(IsReference = true)]
public class Webhook : EntityBase
{
    private string _url;
    private WebHookEvent _webHookEvent;
    private string _entityName;
    private Guid _entityKey;

    public Webhook(string url, WebHookEvent webHookEvent, string entityName, Guid entityKey)
    {
        _url = url;
        _webHookEvent = webHookEvent;
        _entityName = entityName;
        _entityKey = entityKey;
    }

    [DataMember]
    public string Url
    {
        get => _url;
        set => SetPropertyValueAndDetectChanges(value, ref _url!, nameof(Url));
    }

    [DataMember]
    public WebHookEvent Event
    {
        get => _webHookEvent;
        set => SetPropertyValueAndDetectChanges(value, ref _webHookEvent!, nameof(Event));
    }

    [DataMember]
    public string EntityName
    {
        get => _entityName;
        set => SetPropertyValueAndDetectChanges(value, ref _entityName!, nameof(EntityName));
    }

    [DataMember]
    public Guid EntityKey
    {
        get => _entityKey;
        set => SetPropertyValueAndDetectChanges(value, ref _entityKey, nameof(EntityKey));
    }
}
