import { UMB_DOCUMENT_ENTITY_TYPE } from '../entity.js';
import type { UmbDocumentSearchItemModel } from './document.search-provider.js';
import type { UmbSearchDataSource, UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { DocumentService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Rollback that fetches data from the server
 * @export
 * @class UmbDocumentSearchServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbDocumentSearchServerDataSource implements UmbSearchDataSource<UmbDocumentSearchItemModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDocumentSearchServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbDocumentSearchServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Get a list of versions for a document
	 * @returns {*}
	 * @memberof UmbDocumentSearchServerDataSource
	 */
	async search(args: UmbSearchRequestArgs) {
		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			DocumentService.getItemDocumentSearch({
				query: args.query,
			}),
		);

		if (data) {
			const mappedItems: Array<UmbDocumentSearchItemModel> = data.items.map((item) => {
				return {
					href: '/section/content/workspace/document/edit/' + item.id,
					entityType: UMB_DOCUMENT_ENTITY_TYPE,
					unique: item.id,
					isTrashed: item.isTrashed,
					isProtected: item.isProtected,
					documentType: {
						unique: item.documentType.id,
						icon: item.documentType.icon,
						collection: item.documentType.collection ? { unique: item.documentType.collection.id } : null,
					},
					variants: item.variants.map((variant) => {
						return {
							culture: variant.culture || null,
							name: variant.name,
							state: variant.state,
						};
					}),
					name: item.variants[0]?.name, // TODO: this is not correct. We need to get it from the variants. This is a temp solution.
				};
			});

			return { data: { items: mappedItems, total: data.total } };
		}

		return { error };
	}
}
