import { UMB_DOCUMENT_TYPE_ENTITY_TYPE } from '../entity.js';
import type { UmbDocumentTypeSearchItemModel } from './document-type.search-provider.js';
import type { UmbDocumentTypeSearchRequestArgs } from './types.js';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { DocumentTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDataSourceResponse, UmbPagedModel } from '@umbraco-cms/backoffice/repository';
import type { UmbSearchDataSource } from '@umbraco-cms/backoffice/search';

/**
 * A data source for the Rollback that fetches data from the server
 * @class UmbDocumentTypeSearchServerDataSource
 * @implements {UmbSearchDataSource}
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
	 * @returns {Promise<UmbDataSourceResponse<UmbPagedModel<UmbDocumentTypeSearchItemModel>>>} The search results.
	 * @memberof UmbDocumentTypeSearchServerDataSource
	 */
	async search(
		args: UmbDocumentTypeSearchRequestArgs,
	): Promise<UmbDataSourceResponse<UmbPagedModel<UmbDocumentTypeSearchItemModel>>> {
		const { data, error } = await tryExecute(
			this.#host,
			DocumentTypeService.getItemDocumentTypeSearch({
				query: {
					query: args.query,
					isElement: args.isElementType,
					skip: args.paging?.skip,
					take: args.paging?.take,
				},
			}),
		);

		if (data) {
			const mappedItems: Array<UmbDocumentTypeSearchItemModel> = data.items.map((item) => {
				return {
					href: 'section/settings/workspace/document-type/edit/' + item.id,
					entityType: UMB_DOCUMENT_TYPE_ENTITY_TYPE,
					isElement: item.isElement,
					icon: item.icon ?? undefined,
					unique: item.id,
					name: item.name,
				};
			});

			return { data: { items: mappedItems, total: data.total } };
		}

		return { error };
	}
}
