import { DocumentResource } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * @export
 * @class UmbDocumentReferenceServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbDocumentReferenceServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDocumentReferenceServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbDocumentReferenceServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Fetches the item for the given id from the server
	 * @param {Array<string>} ids
	 * @return {*}
	 * @memberof UmbDocumentReferenceServerDataSource
	 */
	async getReferenceById(id: string, skip = 0, take = 20) {
		return await tryExecuteAndNotify(this.#host, DocumentResource.getDocumentByIdReferencedBy({ id, skip, take }));
	}
}
