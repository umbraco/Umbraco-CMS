import { UMB_DATA_TYPE_ENTITY_TYPE } from '../entity.js';
import type { UmbDataTypeSearchItemModel } from './data-type.search-provider.js';
import type { UmbSearchDataSource, UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { DataTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Rollback that fetches data from the server
 * @class UmbDataTypeSearchServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbDataTypeSearchServerDataSource implements UmbSearchDataSource<UmbDataTypeSearchItemModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDataTypeSearchServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDataTypeSearchServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Get a list of versions for a data
	 * @param args
	 * @returns {*}
	 * @memberof UmbDataTypeSearchServerDataSource
	 */
	async search(args: UmbSearchRequestArgs) {
		const { data, error } = await tryExecute(
			this.#host,
			DataTypeService.getItemDataTypeSearch({
				query: { query: args.query },
			}),
		);

		if (data) {
			const mappedItems: Array<UmbDataTypeSearchItemModel> = data.items.map((item) => {
				return {
					href: '/section/settings/workspace/data-type/edit/' + item.id,
					entityType: UMB_DATA_TYPE_ENTITY_TYPE,
					unique: item.id,
					name: item.name,
					propertyEditorSchemaAlias: item.editorAlias,
					propertyEditorUiAlias: item.editorUiAlias || '',
				};
			});

			return { data: { items: mappedItems, total: data.total } };
		}

		return { error };
	}
}
