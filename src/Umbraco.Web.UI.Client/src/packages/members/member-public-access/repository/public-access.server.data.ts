import { DocumentService } from '@umbraco-cms/backoffice/external/backend-api';
import type { PublicAccessRequestModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Document Public Access that fetches data from the server
 * @class UmbDocumentPublicAccessServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbDocumentPublicAccessServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDocumentPublicAccessServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
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
		return tryExecute(this.#host, DocumentService.postDocumentByIdPublicAccess({ path: { id: unique }, body: data }));
	}

	/**
	 * Fetches the Public Access for the given Document unique
	 * @param {string} unique
	 * @memberof UmbDocumentPublicAccessServerDataSource
	 */
	async read(unique: string) {
		if (!unique) throw new Error('unique is missing');
		// NOTE: The entity will not be present, when fetching Public Access for a descendant of a protected Document.
		//       This is a perfectly valid scenario, which is handled in the view. In other words, just use tryExecute here.
		return tryExecute(this.#host, DocumentService.getDocumentByIdPublicAccess({ path: { id: unique } }));
	}

	/**
	 * Updates Public Access for the given Document unique
	 * @param {string} unique
	 * @param {PublicAccessRequestModel} data
	 * @param body
	 * @memberof UmbDocumentPublicAccessServerDataSource
	 */
	async update(unique: string, body: PublicAccessRequestModel) {
		if (!unique) throw new Error('unique is missing');
		return tryExecute(this.#host, DocumentService.putDocumentByIdPublicAccess({ path: { id: unique }, body }));
	}

	/**
	 * Deletes Public Access for the given Document unique
	 * @param {string} unique
	 * @memberof UmbDocumentPublicAccessServerDataSource
	 */
	async delete(unique: string) {
		if (!unique) throw new Error('unique is missing');
		return tryExecute(this.#host, DocumentService.deleteDocumentByIdPublicAccess({ path: { id: unique } }));
	}
}
