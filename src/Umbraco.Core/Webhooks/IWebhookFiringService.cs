﻿namespace Umbraco.Cms.Core.Webhooks;

public interface IWebhookFiringService
{
    Task<HttpResponseMessage> Fire( string url, string eventName, object? requestObject);
}