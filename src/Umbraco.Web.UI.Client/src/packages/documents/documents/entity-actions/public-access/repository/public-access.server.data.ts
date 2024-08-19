import { DocumentService } from '@umbraco-cms/backoffice/external/backend-api';
import type { PublicAccessRequestModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Document Public Access that fetches data from the server
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
	 * Creates the Public Access for the given Document unique
	 * @param {string} unique
	 * @param {PublicAccessRequestModel} data
	 * @memberof UmbDocumentPublicAccessServerDataSource
	 */
	async create(unique: string, data: PublicAccessRequestModel) {
		if (!unique) throw new Error('unique is missing');
		return tryExecuteAndNotify(
			this.#host,
			DocumentService.postDocumentByIdPublicAccess({ id: unique, requestBody: data }),
		);
	}

	/**
	 * Fetches the Public Access for the given Document unique
	 * @param {string} unique
	 * @memberof UmbDocumentPublicAccessServerDataSource
	 */
	async read(unique: string) {
		if (!unique) throw new Error('unique is missing');
		return tryExecuteAndNotify(this.#host, DocumentService.getDocumentByIdPublicAccess({ id: unique }));
	}

	/**
	 * Updates Public Access for the given Document unique
	 * @param {string} unique
	 * @param {PublicAccessRequestModel} data
	 * @memberof UmbDocumentPublicAccessServerDataSource
	 */
	async update(unique: string, requestBody: PublicAccessRequestModel) {
		if (!unique) throw new Error('unique is missing');
		return tryExecuteAndNotify(this.#host, DocumentService.putDocumentByIdPublicAccess({ id: unique, requestBody }));
	}

	/**
	 * Deletes Public Access for the given Document unique
	 * @param {string} unique
	 * @memberof UmbDocumentPublicAccessServerDataSource
	 */
	async delete(unique: string) {
		if (!unique) throw new Error('unique is missing');
		return tryExecuteAndNotify(this.#host, DocumentService.deleteDocumentByIdPublicAccess({ id: unique }));
	}
}
