import { UMB_DOCUMENT_TYPE_ENTITY_TYPE } from '../entity.js';
import type { UmbDocumentTypeSearchItemModel } from './document-type.search-provider.js';
import type { UmbDocumentTypeSearchRequestArgs } from './types.js';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { DocumentTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbSearchDataSource } from '@umbraco-cms/backoffice/search';

/**
 * A data source for the Rollback that fetches data from the server
 * @class UmbDocumentTypeSearchServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbDocumentTypeSearchServerDataSource implements UmbSearchDataSource<UmbDocumentTypeSearchItemModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDocumentTypeSearchServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDocumentTypeSearchServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Get a list of versions for a data
	 * @param {UmbDocumentTypeSearchRequestArgs} args - The arguments for the search
	 * @returns {*}
	 * @memberof UmbDocumentTypeSearchServerDataSource
	 */
	async search(args: UmbDocumentTypeSearchRequestArgs) {
		const { data, error } = await tryExecute(
			this.#host,
			DocumentTypeService.getItemDocumentTypeSearch({
				query: { query: args.query, isElement: args.elementTypesOnly },
			}),
		);

		if (data) {
			const mappedItems: Array<UmbDocumentTypeSearchItemModel> = data.items.map((item) => {
				return {
					href: '/section/settings/workspace/document-type/edit/' + item.id,
					entityType: UMB_DOCUMENT_TYPE_ENTITY_TYPE,
					isElement: item.isElement,
					icon: item.icon,
					unique: item.id,
					name: item.name,
				};
			});

			return { data: { items: mappedItems, total: data.total } };
		}

		return { error };
	}
}
