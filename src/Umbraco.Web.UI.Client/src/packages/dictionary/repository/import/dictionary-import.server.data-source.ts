import type { ImportDictionaryRequestModel } from '@umbraco-cms/backoffice/backend-api';
import { DictionaryResource } from '@umbraco-cms/backoffice/backend-api';
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
	import(temporaryFileUnique: string, parentUnique: string | null) {
		if (!temporaryFileUnique) throw new Error('temporaryFileUnique is required');
		if (parentUnique === undefined) throw new Error('parentUnique is required');

		const requestBody: ImportDictionaryRequestModel = {
			temporaryFile: { id: temporaryFileUnique },
			parent: parentUnique ? { id: parentUnique } : null,
		};

		return tryExecuteAndNotify(
			this.#host,
			DictionaryResource.postDictionaryImport({
				requestBody,
			}),
		);
	}
}
