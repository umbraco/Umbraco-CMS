// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders;

public class WebhookBuilder
    : BuilderBase<IWebhook>,
        IWithIdBuilder,
        IWithKeyBuilder
{
    private int? _id;
    private Guid? _key;
    private string? _url;
    private bool? _enabled;
    private Guid[] _entityKeys;
    private string[]? _events;
    private Dictionary<string, string>? _headers;

    int? IWithIdBuilder.Id
    {
        get => _id;
        set => _id = value;
    }

    Guid? IWithKeyBuilder.Key
    {
        get => _key;
        set => _key = value;
    }

    public WebhookBuilder WithUrl(string url)
    {
        _url = url;
        return this;
    }

    public WebhookBuilder WithEnabled(bool enabled)
    {
        _enabled = enabled;
        return this;
    }

    public WebhookBuilder WithEntityKeys(Guid[] entityKeys)
    {
        _entityKeys = entityKeys;
        return this;
    }

    public WebhookBuilder WithEvents(string[] events)
    {
        _events = events;
        return this;
    }

    public WebhookBuilder WithHeaders(Dictionary<string, string> headers)
    {
        _headers = headers;
        return this;
    }

    public override Webhook Build()
    {
        var id = _id ?? 1;
        var key = _key ?? Guid.NewGuid();
        var url = _url ?? "https://example.com";
        var enabled = _enabled ?? true;
        var entityKeys = _entityKeys;
        var events = _events;
        var headers = _headers;

        return new Webhook(url, enabled, entityKeys, events, headers)
        {
            Id = id,
            Key = key
        };
    }
}
