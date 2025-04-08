import { DocumentService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UpdateDomainsRequestModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Document Culture and Hostnames that fetches data from the server
 * @class UmbDocumentCultureAndHostnamesServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbDocumentCultureAndHostnamesServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDocumentCultureAndHostnamesServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDocumentCultureAndHostnamesServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Fetches the Culture and Hostnames for the given Document unique
	 * @param {string} unique
	 * @memberof UmbDocumentCultureAndHostnamesServerDataSource
	 */
	async read(unique: string) {
		if (!unique) throw new Error('Unique is missing');
		return tryExecute(this.#host, DocumentService.getDocumentByIdDomains({ id: unique }));
	}

	/**
	 * Updates Culture and Hostnames for the given Document unique
	 * @param {string} unique
	 * @param {UpdateDomainsRequestModel} data
	 * @memberof UmbDocumentCultureAndHostnamesServerDataSource
	 */
	async update(unique: string, data: UpdateDomainsRequestModel) {
		if (!unique) throw new Error('Unique is missing');
		return tryExecute(this.#host, DocumentService.putDocumentByIdDomains({ id: unique, requestBody: data }));
	}
}
