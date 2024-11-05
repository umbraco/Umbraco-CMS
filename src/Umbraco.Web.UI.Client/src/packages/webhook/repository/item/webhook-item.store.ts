import type { UmbWebhookItemModel } from './types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStoreBase } from '@umbraco-cms/backoffice/store';

/**
 * @class UmbWebhookItemStore
 * @augments {UmbItemStoreBase}
 * @description - Data Store for Webhook items
 */

export class UmbWebhookItemStore extends UmbItemStoreBase<UmbWebhookItemModel> {
	/**
	 * Creates an instance of UmbWebhookItemStore.
	 * @param {UmbControllerHost} host - The controller host
	 * @memberof UmbWebhookItemStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_WEBHOOK_ITEM_STORE_CONTEXT.toString());
	}
}

export default UmbWebhookItemStore;

export const UMB_WEBHOOK_ITEM_STORE_CONTEXT = new UmbContextToken<UmbWebhookItemStore>('UmbWebhookItemStore');
