import { MediaTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

/**
 * Export Media Server Data Source
 * @class UmbExportMediaTypeServerDataSource
 */
export class UmbExportMediaTypeServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbExportMediaTypeServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbExportMediaTypeServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Export an item for the given id to the destination unique
	 * @param {unique} unique The unique identifier of the item to export.
	 * @returns {UmbDataSourceResponse<Blob | File>} The result of the export operation.
	 * @memberof UmbExportMediaTypeServerDataSource
	 */
	async export(unique: string): Promise<UmbDataSourceResponse<Blob | File>> {
		if (!unique) throw new Error('Unique is missing');

		return tryExecute(this.#host, MediaTypeService.getMediaTypeByIdExport({ path: { id: unique } }));
	}
}
