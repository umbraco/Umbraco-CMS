import type { UmbDictionaryCollectionFilterModel, UmbDictionaryCollectionModel } from '../types.js';
import { UMB_DICTIONARY_ENTITY_TYPE } from '../../entity.js';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/collection';
import { DictionaryService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { UmbDataSourceResponse, UmbPagedModel } from '@umbraco-cms/backoffice/repository';

/**
 * A data source that fetches the dictionary collection data from the server.
 * @class UmbDictionaryCollectionServerDataSource
 * @implements {UmbCollectionDataSource}
 */
export class UmbDictionaryCollectionServerDataSource implements UmbCollectionDataSource<UmbDictionaryCollectionModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDictionaryCollectionServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDictionaryCollectionServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Gets the dictionary collection filtered by the given filter.
	 * @param {UmbDictionaryCollectionFilterModel} query - The filter to apply to the collection.
	 * @returns {Promise<UmbDataSourceResponse<UmbPagedModel<UmbDictionaryCollectionModel>>>} The dictionary collection.
	 * @memberof UmbDictionaryCollectionServerDataSource
	 */
	async getCollection(
		query: UmbDictionaryCollectionFilterModel,
	): Promise<UmbDataSourceResponse<UmbPagedModel<UmbDictionaryCollectionModel>>> {
		const { data, error } = await tryExecute(this.#host, DictionaryService.getDictionary({ query }));

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
