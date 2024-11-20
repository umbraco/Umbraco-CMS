import type {
	UmbDataSourceErrorResponse,
	UmbDataSourceResponse,
	UmbDetailDataSource,
} from '@umbraco-cms/backoffice/repository';
import type { UmbClipboardEntry } from '../types.js';
import { UmbClipboardLocalStorageManager } from '../clipboard-local-storage.manager.js';

/**
 * Manage clipboard entries in local storage
 * @export
 * @class UmbClipboardDetailLocalStorageDataSource
 * @implements {UmbDetailDataSource<UmbClipboardEntry>}
 */
export class UmbClipboardDetailLocalStorageDataSource implements UmbDetailDataSource<UmbClipboardEntry> {
	#localStorageManager = new UmbClipboardLocalStorageManager();

	/**
	 * Scaffold a new clipboard entry
	 * @param {Partial<UmbClipboardEntry>} [preset={}]
	 * @return {*}
	 * @memberof UmbClipboardDetailLocalStorageDataSource
	 */
	async createScaffold(preset: Partial<UmbClipboardEntry> = {}) {
		const data: UmbClipboardEntry = {
			data: [],
			icons: [],
			meta: {},
			name: '',
			type: '',
			unique: '',
			...preset,
		};

		return { data };
	}

	/**
	 * Create a new clipboard entry in local storage
	 * @param {UmbClipboardEntry} model
	 * @return {*}  {Promise<UmbDataSourceResponse<UmbClipboardEntry>>}
	 * @memberof UmbClipboardDetailLocalStorageDataSource
	 */
	async create(model: UmbClipboardEntry): Promise<UmbDataSourceResponse<UmbClipboardEntry>> {
		if (!model) return { error: new Error('Clipboard entry is missing') };

		// check if entry already exists
		const { entries, entry } = this.#localStorageManager.getEntry(model.unique);
		if (entry) return { error: new Error('Clipboard entry already exists') };

		const updatedEntries = [...entries, model];

		this.#localStorageManager.setEntries(updatedEntries);

		return { data: model };
	}

	/**
	 * Read a clipboard entry from local storage
	 * @param {string} unique
	 * @return {*}  {Promise<UmbDataSourceResponse<UmbClipboardEntry>>}
	 * @memberof UmbClipboardDetailLocalStorageDataSource
	 */
	async read(unique: string): Promise<UmbDataSourceResponse<UmbClipboardEntry>> {
		if (!unique) return { error: new Error('Unique is missing') };

		// check if entry exists
		const { entry } = this.#localStorageManager.getEntry(unique);
		if (!entry) return { error: new Error('Clipboard entry not found') };

		return { data: entry };
	}

	/**
	 * Update a clipboard entry in local storage
	 * @param {UmbClipboardEntry} model
	 * @return {*}  {Promise<UmbDataSourceResponse<UmbClipboardEntry>>}
	 * @memberof UmbClipboardDetailLocalStorageDataSource
	 */
	async update(model: UmbClipboardEntry): Promise<UmbDataSourceResponse<UmbClipboardEntry>> {
		if (!model) return { error: new Error('Clipboard entry is missing') };

		// check if entry exists so it can be updated
		const { entry, entries } = this.#localStorageManager.getEntry(model.unique);
		if (!entry) return { error: new Error('Clipboard entry not found') };

		const updatedEntries = entries.map((storedEntry) => (storedEntry.unique === model.unique ? model : storedEntry));

		this.#localStorageManager.setEntries(updatedEntries);
		return { data: model };
	}

	/**
	 * Delete a clipboard entry from local storage
	 * @param {string} unique
	 * @return {*}  {Promise<UmbDataSourceErrorResponse>}
	 * @memberof UmbClipboardDetailLocalStorageDataSource
	 */
	async delete(unique: string): Promise<UmbDataSourceErrorResponse> {
		if (!unique) return { error: new Error('Unique is missing') };

		// check if entry exist so it can be deleted
		const { entry, entries } = this.#localStorageManager.getEntry(unique);
		if (!entry) return { error: new Error('Clipboard entry not found') };

		const updatedEntriesArray = entries.filter((x) => x.unique !== unique);

		this.#localStorageManager.setEntries(updatedEntriesArray);
		return {};
	}
}
