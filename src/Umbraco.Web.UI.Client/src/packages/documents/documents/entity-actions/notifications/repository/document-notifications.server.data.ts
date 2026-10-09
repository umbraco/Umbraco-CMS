import { DocumentService } from '@umbraco-cms/backoffice/external/backend-api';
import type {
	DocumentNotificationResponseModel,
	UpdateDocumentNotificationsRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

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
	 * @returns {Promise<UmbDataSourceResponse<Array<DocumentNotificationResponseModel>>>} The notifications data
	 * @memberof UmbDocumentNotificationsServerDataSource
	 */
	async read(unique: string): Promise<UmbDataSourceResponse<Array<DocumentNotificationResponseModel>>> {
		if (!unique) throw new Error('Unique is missing');
		return tryExecute(this.#host, DocumentService.getDocumentByIdNotifications({ path: { id: unique } }));
	}

	/**
	 * Updates Culture and Hostnames for the given Document unique
	 * @param {string} unique - The unique identifier of the Document
	 * @param {UpdateDocumentNotificationsRequestModel} data - The data to update
	 * @returns {Promise<UmbDataSourceResponse<unknown>>} The result of the update request
	 * @memberof UmbDocumentNotificationsServerDataSource
	 */
	async update(unique: string, data: UpdateDocumentNotificationsRequestModel): Promise<UmbDataSourceResponse<unknown>> {
		if (!unique) throw new Error('Unique is missing');
		return tryExecute(this.#host, DocumentService.putDocumentByIdNotifications({ path: { id: unique }, body: data }));
	}
}
