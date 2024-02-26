import { DocumentResource } from '@umbraco-cms/backoffice/external/backend-api';
import type { DomainsPresentationModelBaseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Document Culture and Hostnames that fetches data from the server
 * @export
 * @class UmbDocumentCultureAndHostnamesServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbDocumentCultureAndHostnamesServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDocumentCultureAndHostnamesServerDataSource.
	 * @param {UmbControllerHost} host
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
		return tryExecuteAndNotify(this.#host, DocumentResource.getDocumentByIdDomains({ id: unique }));
	}

	/**
	 * Updates Culture and Hostnames for the given Document unique
	 * @param {string} unique
	 * @param {DomainsPresentationModelBaseModel} data
	 * @memberof UmbDocumentCultureAndHostnamesServerDataSource
	 */
	async update(unique: string, data: DomainsPresentationModelBaseModel) {
		if (!unique) throw new Error('Unique is missing');
		return tryExecuteAndNotify(this.#host, DocumentResource.putDocumentByIdDomains({ id: unique, requestBody: data }));
	}
}
