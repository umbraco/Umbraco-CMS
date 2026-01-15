import type { UmbWebhookEventModel } from '../types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbStoreBase } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

/**
 * @class UmbWebhookEventStore
 * @augments {UmbStoreBase}
 * @description - Data Store for Webhook Events
 */
export class UmbWebhookEventStore extends UmbStoreBase<UmbWebhookEventModel> {
	/**
	 * Creates an instance of UmbWebhookEventStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbWebhookEventStore
	 */
	constructor(host: UmbControllerHost) {
		super(
			host,
			UMB_WEBHOOK_EVENT_STORE_CONTEXT.toString(),
			new UmbArrayState<UmbWebhookEventModel>([], (x) => x.alias),
		);
	}
}

export default UmbWebhookEventStore;

export const UMB_WEBHOOK_EVENT_STORE_CONTEXT = new UmbContextToken<UmbWebhookEventStore>('UmbWebhookEventStore');
