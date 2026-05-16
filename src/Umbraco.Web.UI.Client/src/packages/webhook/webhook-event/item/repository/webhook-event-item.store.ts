import type { UmbWebhookEventItemModel } from './types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStoreBase } from '@umbraco-cms/backoffice/store';

/**
 * @class UmbWebhookEventItemStore
 * @augments {UmbItemStoreBase}
 * @description - Data Store for Webhook Event items
 */
export class UmbWebhookEventItemStore extends UmbItemStoreBase<UmbWebhookEventItemModel> {
	/**
	 * Creates an instance of UmbWebhookEventItemStore.
	 * @param {UmbControllerHost} host - The controller host
	 * @memberof UmbWebhookEventItemStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_WEBHOOK_EVENT_ITEM_STORE_CONTEXT);
	}
}

export default UmbWebhookEventItemStore;

export const UMB_WEBHOOK_EVENT_ITEM_STORE_CONTEXT = new UmbContextToken<UmbWebhookEventItemStore>(
	'UmbWebhookEventItemStore',
);
