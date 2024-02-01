import type { UmbLanguageCollectionFilterModel, UmbLanguageDetailModel } from '../../types.js';
import { UMB_LANGUAGE_ENTITY_TYPE } from '../../entity.js';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/repository';
import type { LanguageResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { LanguageResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source that fetches the language collection data from the server.
 * @export
 * @class UmbLanguageCollectionServerDataSource
 * @implements {UmbCollectionDataSource}
 */
export class UmbLanguageCollectionServerDataSource implements UmbCollectionDataSource<UmbLanguageDetailModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbLanguageCollectionServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbLanguageCollectionServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Gets the language collection filtered by the given filter.
	 * @param {UmbLanguageCollectionFilterModel} filter
	 * @return {*}
	 * @memberof UmbLanguageCollectionServerDataSource
	 */
	async getCollection(filter: UmbLanguageCollectionFilterModel) {
		debugger;
		const { data, error } = await tryExecuteAndNotify(this.#host, LanguageResource.getLanguage(filter));

		if (error) {
			return { error };
		}

		const { items, total } = data;

		const mappedItems: Array<UmbLanguageDetailModel> = items.map((item: LanguageResponseModel) => {
			const languageDetail: UmbLanguageDetailModel = {
				entityType: UMB_LANGUAGE_ENTITY_TYPE,
				...item,
			};

			return languageDetail;
		});

		return { data: { items: mappedItems, total } };
	}
}
