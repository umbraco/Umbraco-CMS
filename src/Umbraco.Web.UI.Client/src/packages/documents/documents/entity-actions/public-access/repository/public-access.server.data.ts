import { DocumentResource } from '@umbraco-cms/backoffice/backend-api';
import type { PublicAccessRequestModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Document Public Access that fetches data from the server
 * @export
 * @class UmbDocumentPublicAccessServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbDocumentPublicAccessServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDocumentPublicAccessServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbDocumentPublicAccessServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Creates the Public Access for the given Document id
	 * @param {string} id
	 * @param {PublicAccessRequestModel} data
	 * @memberof UmbDocumentPublicAccessServerDataSource
	 */
	async create(id: string, data: PublicAccessRequestModel) {
		if (!id) throw new Error('Id is missing');
		return tryExecuteAndNotify(this.#host, DocumentResource.postDocumentByIdPublicAccess({ id, requestBody: data }));
	}

	/**
	 * Fetches the Public Access for the given Document id
	 * @param {string} id
	 * @memberof UmbDocumentPublicAccessServerDataSource
	 */
	async read(id: string) {
		if (!id) throw new Error('Id is missing');
		return tryExecuteAndNotify(this.#host, DocumentResource.getDocumentByIdPublicAccess({ id }));
	}

	/**
	 * Updates Public Access for the given Document id
	 * @param {string} id
	 * @param {PublicAccessRequestModel} data
	 * @memberof UmbDocumentPublicAccessServerDataSource
	 */
	async update(id: string, requestBody: PublicAccessRequestModel) {
		if (!id) throw new Error('Id is missing');
		return tryExecuteAndNotify(this.#host, DocumentResource.putDocumentByIdPublicAccess({ id, requestBody }));
	}

	/**
	 * Deletes Public Access for the given Document id
	 * @param {string} id
	 * @memberof UmbDocumentPublicAccessServerDataSource
	 */
	async delete(id: string) {
		if (!id) throw new Error('Id is missing');
		return tryExecuteAndNotify(this.#host, DocumentResource.deleteDocumentByIdPublicAccess({ id }));
	}
}
