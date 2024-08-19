import type { UmbWebhookDetailModel } from '../../types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbDetailStoreBase } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * @class UmbWebhookDetailStore
 * @augments {UmbStoreBase}
 * @description - Data Store for Webhook Details
 */
export class UmbWebhookDetailStore extends UmbDetailStoreBase<UmbWebhookDetailModel> {
	/**
	 * Creates an instance of UmbWebhookDetailStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbWebhookDetailStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_WEBHOOK_DETAIL_STORE_CONTEXT.toString());
	}
}

export default UmbWebhookDetailStore;

export const UMB_WEBHOOK_DETAIL_STORE_CONTEXT = new UmbContextToken<UmbWebhookDetailStore>('UmbWebhookDetailStore');
