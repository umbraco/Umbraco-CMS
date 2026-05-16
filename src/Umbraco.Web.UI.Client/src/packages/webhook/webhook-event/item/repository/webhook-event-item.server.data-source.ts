import { UMB_WEBHOOK_EVENT_ENTITY_TYPE } from '../../entity.js';
import type { UmbWebhookEventItemModel } from './types.js';
import type { WebhookEventResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { WebhookService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

/**
 * A server data source for Webhook Event items.
 * @class UmbWebhookEventItemServerDataSource
 * @implements {UmbItemServerDataSourceBase}
 */
export class UmbWebhookEventItemServerDataSource extends UmbItemServerDataSourceBase<
	WebhookEventResponseModel,
	UmbWebhookEventItemModel
> {
	/**
	 * Creates an instance of UmbWebhookEventItemServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbWebhookEventItemServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		super(host, { mapper });
	}

	override async getItems(uniques: Array<string>) {
		if (!uniques.length) return { data: [], error: undefined };

		const { data, error } = await tryExecute(this, WebhookService.getWebhookEvents());

		if (error || !data) {
			return { data: undefined, error };
		}

		const filtered = data.items.filter((item) => uniques.includes(item.alias));
		return { data: this._getMappedItems(filtered), error: undefined };
	}
}

const mapper = (item: WebhookEventResponseModel): UmbWebhookEventItemModel => {
	return {
		entityType: UMB_WEBHOOK_EVENT_ENTITY_TYPE,
		unique: item.alias,
		name: item.eventName,
		eventType: item.eventType,
		alias: item.alias,
	};
};
