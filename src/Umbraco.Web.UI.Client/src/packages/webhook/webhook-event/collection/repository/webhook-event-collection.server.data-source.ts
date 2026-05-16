import type { UmbWebhookEventCollectionFilterModel, UmbWebhookEventCollectionItemModel } from '../types.js';
import { UMB_WEBHOOK_EVENT_ENTITY_TYPE } from '../../entity.js';
import { WebhookService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/collection';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

/**
 * A data source that fetches the webhook event collection data from the server.
 * @class UmbWebhookEventCollectionServerDataSource
 * @implements {UmbCollectionDataSource}
 */
export class UmbWebhookEventCollectionServerDataSource
	extends UmbControllerBase
	implements UmbCollectionDataSource<UmbWebhookEventCollectionItemModel, UmbWebhookEventCollectionFilterModel>
{
	/**
	 * Gets the Webhook Event collection filtered by the given filter.
	 * @param {UmbWebhookEventCollectionFilterModel} filter
	 * @returns {*}
	 * @memberof UmbWebhookEventCollectionServerDataSource
	 */
	async getCollection(filter: UmbWebhookEventCollectionFilterModel) {
		const { data, error } = await tryExecute(
			this,
			WebhookService.getWebhookEvents({ query: { skip: filter.skip, take: filter.take } }),
		);

		if (error || !data) {
			return { error };
		}

		const items = data.items.map((item): UmbWebhookEventCollectionItemModel => {
			return {
				entityType: UMB_WEBHOOK_EVENT_ENTITY_TYPE,
				unique: item.alias,
				name: item.eventName,
				eventType: item.eventType,
				alias: item.alias,
			};
		});

		return { data: { items, total: data.total } };
	}
}
