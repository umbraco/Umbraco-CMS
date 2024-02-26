import type { UmbWebhookCollectionFilterModel } from '../types.js';
import type { UmbWebhookDetailModel } from '../../types.js';
import { UMB_WEBHOOK_ENTITY_TYPE } from '../../entity.js';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/collection';
import { WebhookResource } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source that fetches the webhook collection data from the server.
 * @export
 * @class UmbWebhookCollectionServerDataSource
 * @implements {UmbCollectionDataSource}
 */
export class UmbWebhookCollectionServerDataSource implements UmbCollectionDataSource<UmbWebhookDetailModel> {
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
	 * @param {UmbWebhookeCollectionFilterModel} filter
	 * @return {*}
	 * @memberof UmbWebhookeCollectionServerDataSource
	 */
	async getCollection(filter: UmbWebhookCollectionFilterModel) {
		const { data, error } = await tryExecuteAndNotify(this.#host, WebhookResource.getWebhook(filter));

		if (data) {
			const items = data.items.map((item) => {
				const model: UmbWebhookDetailModel = {
					unique: item.isoCode.toLowerCase(),
					name: item.name,
					entityType: UMB_WEBHOOK_ENTITY_TYPE,
					isDefault: item.isDefault,
					isMandatory: item.isMandatory,
					fallbackIsoCode: item.fallbackIsoCode?.toLowerCase() || null,
				};

				return model;
			});

			return { data: { items, total: data.total } };
		}

		return { error };
	}
}
