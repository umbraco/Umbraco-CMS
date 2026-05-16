import { UmbWebhookEventItemServerDataSource } from './webhook-event-item.server.data-source.js';
import { UMB_WEBHOOK_EVENT_ITEM_STORE_CONTEXT } from './webhook-event-item.store.js';
import type { UmbWebhookEventItemModel } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbWebhookEventItemRepository extends UmbItemRepositoryBase<UmbWebhookEventItemModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbWebhookEventItemServerDataSource, UMB_WEBHOOK_EVENT_ITEM_STORE_CONTEXT);
	}
}

export { UmbWebhookEventItemRepository as api };
