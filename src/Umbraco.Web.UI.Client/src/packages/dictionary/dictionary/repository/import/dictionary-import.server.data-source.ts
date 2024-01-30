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
	 * @param {string} temporaryFileId
	 * @param {string?} parentUnique
	 * @returns {*}
	 * @memberof UmbDictionaryImportServerDataSource
	 */
	import(temporaryFileId: string, parentUnique?: string) {
		return tryExecuteAndNotify(
			this.#host,
			DictionaryResource.postDictionaryImport({ requestBody: { temporaryFileId, parentId: parentUnique } }),
		);
	}
}
