import type { UmbSortChildrenOfArgs } from './types.js';
import { DocumentResource } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A server data source for sorting children of a Document
 * @export
 * @class UmbSortChildrenOfDocumentServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbSortChildrenOfDocumentServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbSortChildrenOfDocumentServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbSortChildrenOfDocumentServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Creates the Public Access for the given Document unique
	 * @param {UmbSortChildrenOfArgs} args
	 * @memberof UmbSortChildrenOfDocumentServerDataSource
	 */
	async sortChildrenOf(args: UmbSortChildrenOfArgs) {
		if (!args.unique) throw new Error('unique is missing');

		const sortingMapping = args.sorting.map((item) => ({ id: item.unique, sortOrder: item.sortOrder }));

		return tryExecuteAndNotify(
			this.#host,
			DocumentResource.putDocumentSort({
				requestBody: {
					parent: {
						id: args.unique,
					},
					sorting: sortingMapping,
				},
			}),
		);
	}
}
