import type { UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';
import { UMB_DATA_TYPE_ENTITY_TYPE } from '../entity.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { DataTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Rollback that fetches data from the server
 * @export
 * @class UmbDataTypeSearchServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbDataTypeSearchServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDataTypeSearchServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbDataTypeSearchServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Get a list of versions for a data
	 * @return {*}
	 * @memberof UmbDataTypeSearchServerDataSource
	 */
	async search(args: UmbSearchRequestArgs) {
		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			DataTypeService.getItemDataTypeSearch({
				query: args.query,
			}),
		);

		if (data) {
			const mappedItems = data.items.map((item) => {
				return {
					entityType: UMB_DATA_TYPE_ENTITY_TYPE,
					unique: item.id,
					name: item.name,
					propertyEditorUiAlias: item.editorUiAlias || '',
				};
			});

			return { data: { items: mappedItems, total: data.total } };
		}

		return { error };
	}
}
