import type { UmbMockWebhookDeliveryModel } from '../../mock-data-set.types.js';

// Webhook IDs referenced from webhook.data.ts
const WEBHOOK_MINIMAL_ID = 'webhook-minimal-id';
const WEBHOOK_NAMED_ID = 'webhook-named-id';

export const data: Array<UmbMockWebhookDeliveryModel> = [
	// webhook-minimal-id — success
	{
		key: 'c0ffee01-0000-0000-0000-000000000001',
		webhookKey: WEBHOOK_MINIMAL_ID,
		statusCode: '200',
		isSuccessStatusCode: true,
		date: '2026-04-15T10:23:41Z',
		eventAlias: 'Umbraco.ContentPublish',
		url: 'https://example.com/webhook',
		retryCount: 0,
		requestHeaders: '{"Content-Type":"application/json","Umb-Webhook-Event":"Umbraco.ContentPublish"}',
		requestBody: '{"Id":"a1b2c3d4-0000-0000-0000-000000000001"}',
		responseHeaders: '{"Content-Type":"application/json"}',
		responseBody: '{"received":true}',
		exceptionOccured: false,
	},
	// webhook-minimal-id — server error with retries
	{
		key: 'c0ffee01-0000-0000-0000-000000000002',
		webhookKey: WEBHOOK_MINIMAL_ID,
		statusCode: '500',
		isSuccessStatusCode: false,
		date: '2026-04-13T14:55:22Z',
		eventAlias: 'Umbraco.ContentPublish',
		url: 'https://example.com/webhook',
		retryCount: 3,
		requestHeaders: '{"Content-Type":"application/json","Umb-Webhook-Event":"Umbraco.ContentPublish","Umb-Webhook-RetryCount":"3"}',
		requestBody: '{"Id":"a1b2c3d4-0000-0000-0000-000000000002"}',
		responseHeaders: '',
		responseBody: 'Internal Server Error',
		exceptionOccured: false,
	},
	// webhook-minimal-id — network exception (no status code)
	{
		key: 'c0ffee01-0000-0000-0000-000000000003',
		webhookKey: WEBHOOK_MINIMAL_ID,
		statusCode: '',
		isSuccessStatusCode: false,
		date: '2026-04-11T16:02:18Z',
		eventAlias: 'Umbraco.ContentPublish',
		url: 'https://example.com/webhook',
		retryCount: 3,
		requestHeaders: '{"Content-Type":"application/json","Umb-Webhook-Event":"Umbraco.ContentPublish","Umb-Webhook-RetryCount":"3"}',
		requestBody: '{"Id":"a1b2c3d4-0000-0000-0000-000000000003"}',
		responseHeaders: '',
		responseBody: '',
		exceptionOccured: true,
	},
	// webhook-named-id — success
	{
		key: 'c0ffee02-0000-0000-0000-000000000001',
		webhookKey: WEBHOOK_NAMED_ID,
		statusCode: '200',
		isSuccessStatusCode: true,
		date: '2026-04-14T08:11:05Z',
		eventAlias: 'Umbraco.ContentPublish',
		url: 'https://example.com/on-publish',
		retryCount: 0,
		requestHeaders: '{"Content-Type":"application/json","Umb-Webhook-Event":"Umbraco.ContentPublish"}',
		requestBody: '{"Id":"a1b2c3d4-0000-0000-0000-000000000004"}',
		responseHeaders: '{"Content-Type":"application/json"}',
		responseBody: '{"received":true}',
		exceptionOccured: false,
	},
	// webhook-named-id — not found
	{
		key: 'c0ffee02-0000-0000-0000-000000000002',
		webhookKey: WEBHOOK_NAMED_ID,
		statusCode: '404',
		isSuccessStatusCode: false,
		date: '2026-04-12T09:30:00Z',
		eventAlias: 'Umbraco.ContentPublish',
		url: 'https://example.com/on-publish',
		retryCount: 1,
		requestHeaders: '{"Content-Type":"application/json","Umb-Webhook-Event":"Umbraco.ContentPublish","Umb-Webhook-RetryCount":"1"}',
		requestBody: '{"Id":"a1b2c3d4-0000-0000-0000-000000000005"}',
		responseHeaders: '',
		responseBody: 'Not Found',
		exceptionOccured: false,
	},
];
