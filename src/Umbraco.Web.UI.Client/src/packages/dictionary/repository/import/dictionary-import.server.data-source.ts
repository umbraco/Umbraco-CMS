import type { ImportDictionaryRequestModel } from '@umbraco-cms/backoffice/external/backend-api';
import { DictionaryService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

export class UmbDictionaryImportServerDataSource {
	#host: UmbControllerHost;

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * @description - Import a dictionary
	 * @param {string} temporaryFileUnique - The unique identifier of the uploaded temporary file to import.
	 * @param {string?} parentUnique - The unique identifier of the parent to import into, if any.
	 * @returns {*} The imported dictionary.
	 * @memberof UmbDictionaryImportServerDataSource
	 */
	async import(temporaryFileUnique: string, parentUnique: string | null) {
		if (!temporaryFileUnique) throw new Error('temporaryFileUnique is required');
		if (parentUnique === undefined) throw new Error('parentUnique is required');

		const body: ImportDictionaryRequestModel = {
			temporaryFile: { id: temporaryFileUnique },
			parent: parentUnique ? { id: parentUnique } : null,
		};

		return tryExecute(
			this.#host,
			DictionaryService.postDictionaryImport({
				body,
			}),
		);
	}
}
