import { UMB_DICTIONARY_ENTITY_TYPE } from '../entity.js';
import type { UmbDictionarySearchItemModel } from './dictionary.search-provider.js';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { DictionaryService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDataSourceResponse, UmbPagedModel } from '@umbraco-cms/backoffice/repository';
import type { UmbSearchDataSource, UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';

/**
 * A data source for the Rollback that fetches data from the server
 * @class UmbDictionarySearchServerDataSource
 * @implements {UmbSearchDataSource<UmbDictionarySearchItemModel>}
 */
export class UmbDictionarySearchServerDataSource implements UmbSearchDataSource<UmbDictionarySearchItemModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDictionarySearchServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDictionarySearchServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Get a list of versions for a data
	 * @param {UmbSearchRequestArgs} args - The arguments for the search
	 * @returns {Promise<UmbDataSourceResponse<UmbPagedModel<UmbDictionarySearchItemModel>>>} The search results.
	 * @memberof UmbDictionarySearchServerDataSource
	 */
	async search(
		args: UmbSearchRequestArgs,
	): Promise<UmbDataSourceResponse<UmbPagedModel<UmbDictionarySearchItemModel>>> {
		const { data, error } = await tryExecute(
			this.#host,
			DictionaryService.getDictionary({
				query: {
					filter: args.query,
					skip: args.paging?.skip,
					take: args.paging?.take,
				},
			}),
		);

		if (data) {
			const mappedItems: Array<UmbDictionarySearchItemModel> = data.items.map((item) => {
				return {
					href: 'section/translation/workspace/dictionary/edit/' + item.id,
					entityType: UMB_DICTIONARY_ENTITY_TYPE,
					unique: item.id,
					name: item.name ?? '',
				};
			});

			return { data: { items: mappedItems, total: data.total } };
		}

		return { error };
	}
}
