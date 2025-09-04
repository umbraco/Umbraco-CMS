/* eslint-disable local-rules/no-direct-api-import */
import { webhookItemCache } from './webhook-item.server.cache.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { WebhookService, type WebhookItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiItemDataRequestManager } from '@umbraco-cms/backoffice/management-api';

export class UmbManagementApiWebhookItemDataRequestManager extends UmbManagementApiItemDataRequestManager<WebhookItemResponseModel> {
	constructor(host: UmbControllerHost) {
		super(host, {
			getItems: (ids: Array<string>) => WebhookService.getItemWebhook({ query: { id: ids } }),
			dataCache: webhookItemCache,
			getUniqueMethod: (item) => item.id,
		});
	}
}
