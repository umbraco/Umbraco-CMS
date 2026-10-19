import { UmbDictionaryExportServerDataSource } from './dictionary-export.server.data-source.js';
import { UmbRepositoryBase, type UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDictionaryExportRepository extends UmbRepositoryBase {
	#exportSource: UmbDictionaryExportServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#exportSource = new UmbDictionaryExportServerDataSource(host);
	}

	/**
	 * @description - Export a Dictionary, optionally including child items.
	 * @param {string} unique - The unique identifier of the dictionary to export.
	 * @param {boolean} [includeChildren] - Whether to include child items in the export.
	 * @returns {Promise<UmbRepositoryResponse<Blob | File>>} The exported dictionary.
	 * @memberof UmbDictionaryExportRepository
	 */
	async requestExport(unique: string, includeChildren = false): Promise<UmbRepositoryResponse<Blob | File>> {
		if (!unique) {
			throw new Error('Unique is missing');
		}

		return this.#exportSource.export(unique, includeChildren);
	}
}

export { UmbDictionaryExportRepository as api };
