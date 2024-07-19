import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { DocumentBlueprintService } from '@umbraco-cms/backoffice/external/backend-api';
import type { CreateDocumentBlueprintFromDocumentRequestModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * A data source for the Document Create Blueprint that fetches data from the server
 * @export
 * @class UmbDocumentCreateBlueprintServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbDocumentCreateBlueprintServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDocumentCreateBlueprintServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbDocumentCreateBlueprintServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Fetches the Culture and Hostnames for the given Document unique
	 * @param {string} unique
	 * @memberof UmbDocumentCreateBlueprintServerDataSource
	 */
	async create(requestBody: CreateDocumentBlueprintFromDocumentRequestModel) {
		return tryExecuteAndNotify(this.#host, DocumentBlueprintService.postDocumentBlueprintFromDocument({ requestBody }));
	}
}
