import { DictionaryService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

export class UmbDictionaryExportServerDataSource {
	#host: UmbControllerHost;

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * @description - Export a Dictionary, optionally including child items.
	 * @param {string} unique - The unique identifier of the dictionary to export.
	 * @param {boolean} includeChildren - Whether to include child items in the export.
	 * @returns {Promise<UmbDataSourceResponse<Blob | File>>} The exported dictionary.
	 * @memberof UmbDictionaryExportServerDataSource
	 */
	async export(unique: string, includeChildren: boolean): Promise<UmbDataSourceResponse<Blob | File>> {
		return await tryExecute(
			this.#host,
			DictionaryService.getDictionaryByIdExport({ path: { id: unique }, query: { includeChildren } }),
		);
	}
}
