import { DocumentService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * @class UmbDocumentReferenceServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbDocumentReferenceServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDocumentReferenceServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDocumentReferenceServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Fetches the item for the given unique from the server
	 * @param {string} id
	 * @returns {*}
	 * @memberof UmbDocumentReferenceServerDataSource
	 */
	async getReferencedBy(id: string, skip = 0, take = 20) {
		return await tryExecuteAndNotify(this.#host, DocumentService.getDocumentByIdReferencedBy({ id, skip, take }));
	}
}
