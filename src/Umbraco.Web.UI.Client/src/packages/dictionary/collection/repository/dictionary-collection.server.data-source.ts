import type { UmbDictionaryCollectionFilterModel, UmbDictionaryCollectionModel } from '../types.js';
import { UMB_DICTIONARY_ENTITY_TYPE } from '../../entity.js';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/repository';
import { DictionaryResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source that fetches the dictionary collection data from the server.
 * @export
 * @class UmbDictionaryCollectionServerDataSource
 * @implements {UmbCollectionDataSource}
 */
export class UmbDictionaryCollectionServerDataSource implements UmbCollectionDataSource<UmbDictionaryCollectionModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDictionaryCollectionServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbDictionaryCollectionServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Gets the dictionary collection filtered by the given filter.
	 * @param {UmbDictionaryCollectionFilterModel} filter
	 * @return {*}
	 * @memberof UmbDictionaryCollectionServerDataSource
	 */
	async getCollection(filter: UmbDictionaryCollectionFilterModel) {
		const { data, error } = await tryExecuteAndNotify(this.#host, DictionaryResource.getDictionary(filter));

		if (data) {
			const items = data.items.map((item) => {
				const model: UmbDictionaryCollectionModel = {
					entityType: UMB_DICTIONARY_ENTITY_TYPE,
					name: item.name!,
					parentUnique: item.parent ? item.parent.id : null,
					translatedIsoCodes: item.translatedIsoCodes,
					unique: item.id,
				};
				return model;
			});

			return { data: { items, total: data.total } };
		}

		return { error };
	}
}
