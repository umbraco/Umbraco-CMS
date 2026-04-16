const { http, HttpResponse } = window.MockServiceWorker;
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';
import { UmbId } from '@umbraco-cms/backoffice/id';

// Sample delivery log entries that represent various delivery outcomes.
// These are generated relative to the webhook ID to keep them realistic.
const buildDeliveryLogs = (webhookId: string) => [
	{
		key: UmbId.new(),
		webhookKey: webhookId,
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
	{
		key: UmbId.new(),
		webhookKey: webhookId,
		statusCode: '200',
		isSuccessStatusCode: true,
		date: '2026-04-14T08:11:05Z',
		eventAlias: 'Umbraco.ContentPublish',
		url: 'https://example.com/webhook',
		retryCount: 0,
		requestHeaders: '{"Content-Type":"application/json","Umb-Webhook-Event":"Umbraco.ContentPublish"}',
		requestBody: '{"Id":"a1b2c3d4-0000-0000-0000-000000000002"}',
		responseHeaders: '{"Content-Type":"application/json"}',
		responseBody: '{"received":true}',
		exceptionOccured: false,
	},
	{
		key: UmbId.new(),
		webhookKey: webhookId,
		statusCode: '500',
		isSuccessStatusCode: false,
		date: '2026-04-13T14:55:22Z',
		eventAlias: 'Umbraco.ContentPublish',
		url: 'https://example.com/webhook',
		retryCount: 3,
		requestHeaders: '{"Content-Type":"application/json","Umb-Webhook-Event":"Umbraco.ContentPublish","Umb-Webhook-RetryCount":"3"}',
		requestBody: '{"Id":"a1b2c3d4-0000-0000-0000-000000000003"}',
		responseHeaders: '',
		responseBody: 'Internal Server Error',
		exceptionOccured: false,
	},
	{
		key: UmbId.new(),
		webhookKey: webhookId,
		statusCode: '404',
		isSuccessStatusCode: false,
		date: '2026-04-12T09:30:00Z',
		eventAlias: 'Umbraco.ContentSaved',
		url: 'https://example.com/webhook',
		retryCount: 1,
		requestHeaders: '{"Content-Type":"application/json","Umb-Webhook-Event":"Umbraco.ContentSaved","Umb-Webhook-RetryCount":"1"}',
		requestBody: '{"Id":"a1b2c3d4-0000-0000-0000-000000000004"}',
		responseHeaders: '',
		responseBody: 'Not Found',
		exceptionOccured: false,
	},
	{
		key: UmbId.new(),
		webhookKey: webhookId,
		statusCode: '',
		isSuccessStatusCode: false,
		date: '2026-04-11T16:02:18Z',
		eventAlias: 'Umbraco.ContentDelete',
		url: 'https://example.com/webhook',
		retryCount: 3,
		requestHeaders: '{"Content-Type":"application/json","Umb-Webhook-Event":"Umbraco.ContentDelete","Umb-Webhook-RetryCount":"3"}',
		requestBody: '{"Id":"a1b2c3d4-0000-0000-0000-000000000005"}',
		responseHeaders: '',
		responseBody: '',
		exceptionOccured: true,
	},
];

export const deliveryHandlers = [
	http.get(umbracoPath(`${UMB_SLUG}/:id/logs`), ({ params, request }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400 });

		const url = new URL(request.url);
		const skipParam = url.searchParams.get('skip');
		const skip = skipParam ? Number.parseInt(skipParam) : 0;
		const takeParam = url.searchParams.get('take');
		const take = takeParam ? Number.parseInt(takeParam) : 10;

		const logs = buildDeliveryLogs(id);
		const paged = logs.slice(skip, skip + take);

		return HttpResponse.json({ items: paged, total: logs.length });
	}),
];
