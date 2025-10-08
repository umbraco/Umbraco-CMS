import { webhookItemCache } from './webhook-item.server.cache.js';
import { UmbManagementApiItemDataCacheInvalidationManager } from '@umbraco-cms/backoffice/management-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { WebhookItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbManagementApiWebhookItemDataCacheInvalidationManager extends UmbManagementApiItemDataCacheInvalidationManager<WebhookItemResponseModel> {
	constructor(host: UmbControllerHost) {
		super(host, {
			dataCache: webhookItemCache,
			eventSources: ['Umbraco:CMS:Webhook'],
		});
	}
}
