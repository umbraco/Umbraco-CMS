import { DocumentTypeService, type PostDocumentTypeImportData } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

/**
 * Document Type Import Server Data Source
 * @Import
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
	 * @param {temporaryUnique} temporaryUnique
	 * @returns {*}
	 * @memberof UmbDocumentTypeImportServerDataSource
	 */
	async import(temporaryUnique: string) {
		if (!temporaryUnique) throw new Error('Unique is missing');

		const requestBody: PostDocumentTypeImportData = {
			requestBody: { file: { id: temporaryUnique } },
		};

		return tryExecute(this.#host, DocumentTypeService.postDocumentTypeImport(requestBody));
	}
}
