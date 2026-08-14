import type { UmbLanguageCollectionFilterModel, UmbLanguageCollectionItemModel } from '../types.js';
import type { UmbLanguageDetailModel } from '../../types.js';
import { UMB_LANGUAGE_ENTITY_TYPE } from '../../entity.js';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/collection';
import { LanguageService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { UmbDataSourceResponse, UmbPagedModel } from '@umbraco-cms/backoffice/repository';

/**
 * A data source that fetches the language collection data from the server.
 * @class UmbLanguageCollectionServerDataSource
 * @implements {UmbCollectionDataSource}
 */
export class UmbLanguageCollectionServerDataSource implements UmbCollectionDataSource<UmbLanguageDetailModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbLanguageCollectionServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbLanguageCollectionServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Gets the language collection filtered by the given filter.
	 * @param {UmbLanguageCollectionFilterModel} filter The filter to apply to the collection.
	 * @returns {UmbDataSourceResponse<UmbPagedModel<UmbLanguageCollectionItemModel>>} The language collection.
	 * @memberof UmbLanguageCollectionServerDataSource
	 */
	async getCollection(
		filter: UmbLanguageCollectionFilterModel,
	): Promise<UmbDataSourceResponse<UmbPagedModel<UmbLanguageCollectionItemModel>>> {
		const { data, error } = await tryExecute(
			this.#host,
			LanguageService.getLanguage({ query: { skip: filter.skip, take: filter.take } }),
		);

		if (data) {
			const items = data.items.map((item) => {
				const model: UmbLanguageCollectionItemModel = {
					unique: item.isoCode,
					name: item.name,
					entityType: UMB_LANGUAGE_ENTITY_TYPE,
					isDefault: item.isDefault,
					isMandatory: item.isMandatory,
					fallbackIsoCode: item.fallbackIsoCode || null,
					icon: 'icon-globe',
				};

				return model;
			});

			return { data: { items, total: data.total } };
		}

		return { error };
	}
}
