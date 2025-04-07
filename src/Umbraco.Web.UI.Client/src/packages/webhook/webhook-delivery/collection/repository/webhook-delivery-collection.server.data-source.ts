import type { UmbWebhookDeliveryCollectionFilterModel } from '../types.js';
import { UMB_WEBHOOK_DELIVERY_ENTITY_TYPE } from '../../../entity.js';
import type { UmbWebhookDeliveryDetailModel } from '../../types.js';
import { WebhookService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

/**
 * A data source that fetches the webhook delivery collection data from the server.
 * @class UmbWebhookDeliveryCollectionServerDataSource
 * @implements {UmbCollectionDataSource}
 */
export class UmbWebhookDeliveryCollectionServerDataSource implements UmbWebhookDeliveryCollectionServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbWebhookDeliveryCollectionServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbWebhookDeliveryCollectionServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Gets the Webhook delivery collection filtered by the given filter.
	 * @param {UmbWebhookDeliveryCollectionFilterModel} filter
	 * @param filter
	 * @returns {*}
	 * @memberof UmbWebhookDeliveryCollectionServerDataSource
	 */
	async getCollection(filter: UmbWebhookDeliveryCollectionFilterModel) {
		const { data, error } = await tryExecute(
			this.#host,
			WebhookService.getWebhookByIdLogs({
				id: filter.webhook.unique,
				skip: filter.skip,
				take: filter.take,
			}),
		);

		if (error || !data) {
			return { error };
		}

		const items = data.items.map((item) => {
			const model: UmbWebhookDeliveryDetailModel = {
				entityType: UMB_WEBHOOK_DELIVERY_ENTITY_TYPE,
				unique: item.key,
				date: item.date,
				url: item.url,
				eventAlias: item.eventAlias,
				retryCount: item.retryCount,
				statusCode: item.statusCode,
			};

			return model;
		});

		return { data: { items, total: data.total } };
	}
}
