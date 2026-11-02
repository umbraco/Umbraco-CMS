import { DocumentTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';
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
	 * @param {unique} unique - The unique identifier of the item to export.
	 * @returns {Promise<UmbDataSourceResponse<Blob | File>>} The exported item.
	 * @memberof UmbExportDocumentTypeServerDataSource
	 */
	async export(unique: string): Promise<UmbDataSourceResponse<Blob | File>> {
		if (!unique) throw new Error('Unique is missing');

		return tryExecute(this.#host, DocumentTypeService.getDocumentTypeByIdExport({ path: { id: unique } }));
	}
}
