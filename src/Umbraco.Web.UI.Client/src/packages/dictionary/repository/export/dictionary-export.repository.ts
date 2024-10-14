import { UmbDictionaryExportServerDataSource } from './dictionary-export.server.data-source.js';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDictionaryExportRepository extends UmbRepositoryBase {
	#exportSource: UmbDictionaryExportServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#exportSource = new UmbDictionaryExportServerDataSource(host);
	}

	/**
	 * @description - Export a Dictionary, optionally including child items.
	 * @param {string} unique
	 * @param {boolean} [includeChildren]
	 * @returns {*}
	 * @memberof UmbDictionaryExportRepository
	 */
	async requestExport(unique: string, includeChildren = false) {
		if (!unique) {
			throw new Error('Unique is missing');
		}

		return this.#exportSource.export(unique, includeChildren);
	}
}

export { UmbDictionaryExportRepository as api };
