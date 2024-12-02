import { DictionaryService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

export class UmbDictionaryExportServerDataSource {
	#host: UmbControllerHost;

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * @description - Export a Dictionary, optionally including child items.
	 * @param {string} unique
	 * @param {boolean} includeChildren
	 * @returns {*}
	 * @memberof UmbDictionaryExportServerDataSource
	 */
	async export(unique: string, includeChildren: boolean) {
		return await tryExecuteAndNotify(
			this.#host,
			DictionaryService.getDictionaryByIdExport({ id: unique, includeChildren }),
		);
	}
}
