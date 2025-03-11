import { UmbWebhookItemServerDataSource } from './webhook-item.server.data-source.js';
import { UMB_WEBHOOK_ITEM_STORE_CONTEXT } from './webhook-item.store.js';
import type { UmbWebhookItemModel } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbWebhookItemRepository extends UmbItemRepositoryBase<UmbWebhookItemModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbWebhookItemServerDataSource, UMB_WEBHOOK_ITEM_STORE_CONTEXT);
	}
}
export default UmbWebhookItemRepository;
