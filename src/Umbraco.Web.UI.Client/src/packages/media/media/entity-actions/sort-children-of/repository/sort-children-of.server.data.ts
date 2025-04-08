import { MediaService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { UmbSortChildrenOfArgs, UmbSortChildrenOfDataSource } from '@umbraco-cms/backoffice/tree';

/**
 * A server data source for sorting children of a Media
 * @class UmbSortChildrenOfMediaServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbSortChildrenOfMediaServerDataSource implements UmbSortChildrenOfDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbSortChildrenOfMediaServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbSortChildrenOfMediaServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Creates the Public Access for the given Media unique
	 * @param {UmbSortChildrenOfArgs} args
	 * @memberof UmbSortChildrenOfMediaServerDataSource
	 */
	async sortChildrenOf(args: UmbSortChildrenOfArgs) {
		if (args.unique === undefined) throw new Error('unique is missing');

		const sortingMapping = args.sorting.map((item) => ({ id: item.unique, sortOrder: item.sortOrder }));

		return tryExecute(
			this.#host,
			MediaService.putMediaSort({
				requestBody: {
					parent: args.unique ? { id: args.unique } : null,
					sorting: sortingMapping,
				},
			}),
		);
	}
}
