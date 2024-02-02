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
	 * @param {string} temporaryFileUnique
	 * @param {string} [parentUnique]
	 * @return {*}
	 * @memberof UmbDictionaryImportRepository
	 */
	import(temporaryFileUnique: string, parentUnique: string | null) {
		if (!temporaryFileUnique) throw new Error('Temporary file unique is missing');
		if (parentUnique === undefined) throw new Error('Parent unique is missing');

		return this.#importSource.import(temporaryFileUnique, parentUnique);
	}
}
