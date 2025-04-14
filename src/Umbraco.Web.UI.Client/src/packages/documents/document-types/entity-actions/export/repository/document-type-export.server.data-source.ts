import { DocumentTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

/**
 * Export Document Server Data Source
 * @class UmbExportDocumentTypeServerDataSource
 */
export class UmbExportDocumentTypeServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbExportDocumentTypeServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbExportDocumentTypeServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Export an item for the given id to the destination unique
	 * @param {unique} unique
	 * @returns {*}
	 * @memberof UmbExportDocumentTypeServerDataSource
	 */
	async export(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		return tryExecute(this.#host, DocumentTypeService.getDocumentTypeByIdExport({ id: unique }));
	}
}
