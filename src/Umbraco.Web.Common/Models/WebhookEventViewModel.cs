﻿using System.Runtime.Serialization;
using Umbraco.Cms.Core.Webhooks;

namespace Umbraco.Cms.Web.Common.Models;

[DataContract]
public class WebhookEventViewModel
{
    [DataMember(Name = "eventName")]
    public string EventName { get; set; } = string.Empty;

    [DataMember(Name = "eventType")]
    public WebhookEventType EventType { get; set; }

    [DataMember(Name = "alias")]
    public string Alias { get; set; } = string.Empty;
}
