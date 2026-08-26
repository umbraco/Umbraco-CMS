import type { ImportDictionaryRequestModel } from '@umbraco-cms/backoffice/external/backend-api';
import { DictionaryService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';
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
	 * @returns {Promise<UmbDataSourceResponse<unknown>>} The result of the import request.
	 * @memberof UmbDictionaryImportServerDataSource
	 */
	async import(temporaryFileUnique: string, parentUnique: string | null): Promise<UmbDataSourceResponse<unknown>> {
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
