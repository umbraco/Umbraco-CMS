import { UMB_DOCUMENT_ENTITY_TYPE } from '../entity.js';
import type { UmbDocumentSearchItemModel } from './types.js';
import type { UmbSearchDataSource, UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { DocumentService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Rollback that fetches data from the server
 * @class UmbDocumentSearchServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbDocumentSearchServerDataSource implements UmbSearchDataSource<UmbDocumentSearchItemModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDocumentSearchServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDocumentSearchServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Get a list of versions for a document
	 * @param args
	 * @returns {*}
	 * @memberof UmbDocumentSearchServerDataSource
	 */
	async search(args: UmbSearchRequestArgs) {
		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			DocumentService.getItemDocumentSearch({
				query: args.query,
				parentId: args.searchFrom?.unique ?? undefined,
			}),
		);

		if (data) {
			const mappedItems: Array<UmbDocumentSearchItemModel> = data.items.map((item) => {
				return {
					documentType: {
						collection: item.documentType.collection ? { unique: item.documentType.collection.id } : null,
						icon: item.documentType.icon,
						unique: item.documentType.id,
					},
					entityType: UMB_DOCUMENT_ENTITY_TYPE,
					hasChildren: item.hasChildren,
					href: '/section/content/workspace/document/edit/' + item.id,
					isProtected: item.isProtected,
					isTrashed: item.isTrashed,
					name: item.variants[0]?.name, // TODO: this is not correct. We need to get it from the variants. This is a temp solution.
					parent: item.parent ? { unique: item.parent.id } : null,
					unique: item.id,
					variants: item.variants.map((variant) => {
						return {
							culture: variant.culture || null,
							name: variant.name,
							state: variant.state,
						};
					}),
				};
			});

			return { data: { items: mappedItems, total: data.total } };
		}

		return { error };
	}
}
