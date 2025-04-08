import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { DocumentBlueprintService } from '@umbraco-cms/backoffice/external/backend-api';
import type { CreateDocumentBlueprintFromDocumentRequestModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * A data source for the Document Create Blueprint that fetches data from the server
 * @class UmbDocumentCreateBlueprintServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbDocumentCreateBlueprintServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDocumentCreateBlueprintServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDocumentCreateBlueprintServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Fetches the Culture and Hostnames for the given Document unique
	 * @param {string} unique
	 * @param requestBody
	 * @memberof UmbDocumentCreateBlueprintServerDataSource
	 */
	async create(requestBody: CreateDocumentBlueprintFromDocumentRequestModel) {
		return tryExecute(this.#host, DocumentBlueprintService.postDocumentBlueprintFromDocument({ requestBody }));
	}
}
