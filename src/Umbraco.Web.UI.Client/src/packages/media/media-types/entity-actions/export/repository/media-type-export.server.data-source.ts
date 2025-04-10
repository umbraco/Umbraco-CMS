import { MediaTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

/**
 * Export Media Server Data Source
 * @export
 * @class UmbExportMediaTypeServerDataSource
 */
export class UmbExportMediaTypeServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbExportMediaTypeServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbExportMediaTypeServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Export an item for the given id to the destination unique
	 * @param {unique} unique
	 * @returns {*}
	 * @memberof UmbExportMediaTypeServerDataSource
	 */
	async export(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		return tryExecute(this.#host, MediaTypeService.getMediaTypeByIdExport({ id: unique }));
	}
}
