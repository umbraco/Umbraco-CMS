import type { UmbWebhookCollectionFilterModel } from '../types.js';
import { UMB_WEBHOOK_ENTITY_TYPE } from '../../../entity.js';
import type { UmbWebhookDetailModel } from '../../types.js';
import { WebhookService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source that fetches the webhook collection data from the server.
 * @class UmbWebhookCollectionServerDataSource
 * @implements {UmbCollectionDataSource}
 */
export class UmbWebhookCollectionServerDataSource implements UmbWebhookCollectionServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbWebhookCollectionServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbWebhookCollectionServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Gets the Webhook collection filtered by the given filter.
	 * @param {UmbWebhookCollectionFilterModel} filter
	 * @param _filter
	 * @returns {*}
	 * @memberof UmbWebhookCollectionServerDataSource
	 */
	async getCollection(_filter: UmbWebhookCollectionFilterModel) {
		const { data, error } = await tryExecuteAndNotify(this.#host, WebhookService.getWebhook(_filter));

		if (error || !data) {
			return { error };
		}

		const items = data.items.map((item) => {
			const model: UmbWebhookDetailModel = {
				entityType: UMB_WEBHOOK_ENTITY_TYPE,
				unique: item.id,
				url: item.url,
				name: item.name,
				description: item.description,
				enabled: item.enabled,
				headers: item.headers,
				events: item.events,
				contentTypes: item.contentTypeKeys,
			};

			return model;
		});

		return { data: { items, total: data.total } };
	}
}
