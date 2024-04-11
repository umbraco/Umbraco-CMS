import type { UmbSearchRequestArgs } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { DocumentService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Rollback that fetches data from the server
 * @export
 * @class UmbDocumentSearchServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbDocumentSearchServerDataSource {
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
	 * @return {*}
	 * @memberof UmbDocumentSearchServerDataSource
	 */
	search(args: UmbSearchRequestArgs) {
		return tryExecuteAndNotify(
			this.#host,
			DocumentService.getItemDocumentSearch({
				query: args.query,
			}),
		);
	}
}
