import type { ImportDictionaryRequestModel } from '@umbraco-cms/backoffice/external/backend-api';
import { DictionaryService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

export class UmbDictionaryImportServerDataSource {
	#host: UmbControllerHost;

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * @description - Import a dictionary
	 * @param {string} temporaryFileUnique
	 * @param {string?} parentUnique
	 * @returns {*}
	 * @memberof UmbDictionaryImportServerDataSource
	 */
	async import(temporaryFileUnique: string, parentUnique: string | null) {
		if (!temporaryFileUnique) throw new Error('temporaryFileUnique is required');
		if (parentUnique === undefined) throw new Error('parentUnique is required');

		const requestBody: ImportDictionaryRequestModel = {
			temporaryFile: { id: temporaryFileUnique },
			parent: parentUnique ? { id: parentUnique } : null,
		};

		return tryExecuteAndNotify(
			this.#host,
			DictionaryService.postDictionaryImport({
				requestBody,
			}),
		);
	}
}
