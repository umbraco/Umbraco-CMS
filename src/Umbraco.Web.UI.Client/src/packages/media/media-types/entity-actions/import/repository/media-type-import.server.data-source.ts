import { MediaTypeService, type PostMediaTypeImportData } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

/**
 * Media Type Import Server Data Source
 * @Import
 * @class UmbMediaTypeImportServerDataSource
 */
export class UmbMediaTypeImportServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbMediaTypeImportServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbMediaTypeImportServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Import an item for the given id to the destination unique
	 * @param {temporaryUnique} temporaryUnique
	 * @returns {*}
	 * @memberof UmbMediaTypeImportServerDataSource
	 */
	async import(temporaryUnique: string) {
		if (!temporaryUnique) throw new Error('Unique is missing');

		const requestBody: PostMediaTypeImportData = {
			requestBody: { file: { id: temporaryUnique } },
		};

		return tryExecute(this.#host, MediaTypeService.postMediaTypeImport(requestBody));
	}
}
