import { UmbDictionaryImportServerDataSource } from './dictionary-import.server.data-source.js';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDictionaryImportRepository extends UmbRepositoryBase {
	#importSource: UmbDictionaryImportServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#importSource = new UmbDictionaryImportServerDataSource(host);
	}

	/**
	 * @description - Import a dictionary
	 * @param {string} temporaryFileId
	 * @param {string} [parentUnique]
	 * @return {*}
	 * @memberof UmbDictionaryImportRepository
	 */
	import(temporaryFileId: string, parentUnique?: string) {
		if (!temporaryFileId) {
			throw new Error('Temporary file id is missing');
		}

		return this.#importSource.import(temporaryFileId, parentUnique);
	}
}
