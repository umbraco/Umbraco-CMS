import { DocumentResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Document that fetches data from the server
 * @export
 * @class UmbDocumentServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbDocumentServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDocumentServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbDocumentServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Moves a Document to the recycle bin on the server
	 * @param {string} id
	 * @memberof UmbDocumentServerDataSource
	 */
	async trash(id: string) {
		if (!id) throw new Error('Document ID is missing');
		// TODO: if we get a trash endpoint, we should use that instead.
		return tryExecuteAndNotify(this.#host, DocumentResource.putDocumentByIdMoveToRecycleBin({ id }));
	}
}
