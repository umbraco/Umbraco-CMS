import type { UmbWebhookCollectionFilterModel } from '../types.js';
import type { UmbWebhookDetailModel } from '../../types.js';
import { UMB_WEBHOOK_ENTITY_TYPE } from '../../entity.js';
import { WebhookResource } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source that fetches the webhook collection data from the server.
 * @export
 * @class UmbWebhookCollectionServerDataSource
 * @implements {UmbCollectionDataSource}
 */
export class UmbWebhookCollectionServerDataSource implements UmbWebhookCollectionServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbWebhookCollectionServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbWebhookCollectionServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Gets the Wwbhook collection filtered by the given filter.
	 * @param {UmbWebhookCollectionFilterModel} filter
	 * @return {*}
	 * @memberof UmbWebhookCollectionServerDataSource
	 */
	async getCollection(_filter: UmbWebhookCollectionFilterModel) {
		const { data, error } = await tryExecuteAndNotify(this.#host, WebhookResource.getWebhookItem({}));

		if (data) {
			const items = data.map((item) => {
				const model: UmbWebhookDetailModel = {
					entityType: UMB_WEBHOOK_ENTITY_TYPE,
					unique: item.url,
					name: item.name,
					url: item.url,
					enabled: item.enabled,
					events: item.events.split(','),
					types: item.types.split(','),
				};

				return model;
			});

			return { data: { items, total: data.length } };
		}

		return { error };
	}
}
