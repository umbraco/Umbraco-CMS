import { DocumentService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UpdateDocumentNotificationsRequestModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Document Culture and Hostnames that fetches data from the server
 * @class UmbDocumentNotificationsServerDataSource
 */
export class UmbDocumentNotificationsServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDocumentNotificationsServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDocumentNotificationsServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Fetches the Culture and Hostnames for the given Document unique
	 * @param {string} unique - The unique identifier of the Document
	 * @memberof UmbDocumentNotificationsServerDataSource
	 */
	async read(unique: string) {
		if (!unique) throw new Error('Unique is missing');
		return tryExecute(this.#host, DocumentService.getDocumentByIdNotifications({ path: { id: unique } }));
	}

	/**
	 * Updates Culture and Hostnames for the given Document unique
	 * @param {string} unique - The unique identifier of the Document
	 * @param {UpdateDocumentNotificationsRequestModel} data - The data to update
	 * @memberof UmbDocumentNotificationsServerDataSource
	 */
	async update(unique: string, data: UpdateDocumentNotificationsRequestModel) {
		if (!unique) throw new Error('Unique is missing');
		return tryExecute(this.#host, DocumentService.putDocumentByIdNotifications({ path: { id: unique }, body: data }));
	}
}
