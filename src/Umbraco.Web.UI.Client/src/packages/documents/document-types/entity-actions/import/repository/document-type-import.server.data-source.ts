import { DocumentTypeService, type ImportDocumentTypeRequestModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

/**
 * Document Type Import Server Data Source
 * @class UmbDocumentTypeImportServerDataSource
 */
export class UmbDocumentTypeImportServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDocumentTypeImportServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDocumentTypeImportServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Import an item for the given id to the destination unique
	 * @param {temporaryUnique} temporaryUnique - The unique identifier of the temporary file to import.
	 * @returns {Promise<UmbDataSourceResponse<unknown>>} The result of the import request.
	 * @memberof UmbDocumentTypeImportServerDataSource
	 */
	async import(temporaryUnique: string): Promise<UmbDataSourceResponse<unknown>> {
		if (!temporaryUnique) throw new Error('Unique is missing');

		const body: ImportDocumentTypeRequestModel = {
			file: { id: temporaryUnique },
		};

		return tryExecute(this.#host, DocumentTypeService.postDocumentTypeImport({ body }));
	}
}
