import { DocumentResource, DomainsPresentationModelBaseModel } from '@umbraco-cms/backoffice/backend-api';
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
	 * Fetches the Culture and Hostnames for the given Document id
	 * @param {string} id
	 * @memberof UmbDocumentCultureAndHostnamesServerDataSource
	 */
	async read(id: string) {
		if (!id) throw new Error('Id is missing');
		return tryExecuteAndNotify(this.#host, DocumentResource.getDocumentByIdDomains({ id }));
	}

	/**
	 * Updates Culture and Hostnames for the given Document id
	 * @param {string} id
	 * @param {DomainsPresentationModelBaseModel} data
	 * @memberof UmbDocumentCultureAndHostnamesServerDataSource
	 */
	async update(id: string, data: DomainsPresentationModelBaseModel) {
		if (!id) throw new Error('Id is missing');
		return tryExecuteAndNotify(this.#host, DocumentResource.putDocumentByIdDomains({ id, requestBody: data }));
	}
}
