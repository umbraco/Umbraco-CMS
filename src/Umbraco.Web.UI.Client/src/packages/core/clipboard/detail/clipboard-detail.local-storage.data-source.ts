import type {
	UmbDataSourceErrorResponse,
	UmbDataSourceResponse,
	UmbDetailDataSource,
} from '@umbraco-cms/backoffice/repository';
import type { UmbClipboardEntry } from '../types.js';

const UMB_CLIPBOARD_LOCALSTORAGE_KEY = 'umb:clipboard';

/**
 * Manage clipboard entries in local storage
 * @export
 * @class UmbClipboardDetailLocalStorageDataSource
 * @implements {UmbDetailDataSource<UmbClipboardEntry>}
 */
export class UmbClipboardDetailLocalStorageDataSource implements UmbDetailDataSource<UmbClipboardEntry> {
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
		if (!model) throw new Error('Clipboard entry is missing');
		const items = this.#getEntriesFromLocalStorage();

		// Check if the item already exists
		const existingItem = items.find((x) => x.unique === model.unique);
		if (existingItem) throw new Error(`Clipboard entry with unique ${model.unique} already exists`);

		const updatedItems = [...items, model];
		localStorage.setItem(UMB_CLIPBOARD_LOCALSTORAGE_KEY, JSON.stringify(updatedItems));
		debugger;

		return { data: model };
	}

	/**
	 * Read a clipboard entry from local storage
	 * @param {string} unique
	 * @return {*}  {Promise<UmbDataSourceResponse<UmbClipboardEntry>>}
	 * @memberof UmbClipboardDetailLocalStorageDataSource
	 */
	async read(unique: string): Promise<UmbDataSourceResponse<UmbClipboardEntry>> {
		if (!unique) throw new Error('Unique is missing');
		return { data: this.#getEntryFromLocalStorage(unique) };
	}

	/**
	 * Update a clipboard entry in local storage
	 * @param {UmbClipboardEntry} model
	 * @return {*}  {Promise<UmbDataSourceResponse<UmbClipboardEntry>>}
	 * @memberof UmbClipboardDetailLocalStorageDataSource
	 */
	async update(model: UmbClipboardEntry): Promise<UmbDataSourceResponse<UmbClipboardEntry>> {
		if (!model) throw new Error('Clipboard entry is missing');
		const items = this.#getEntriesFromLocalStorage();
		const updatedItems = items.map((storedEntry) => (storedEntry.unique === model.unique ? model : storedEntry));
		localStorage.setItem(UMB_CLIPBOARD_LOCALSTORAGE_KEY, JSON.stringify(updatedItems));
		return { data: model };
	}

	/**
	 * Delete a clipboard entry from local storage
	 * @param {string} unique
	 * @return {*}  {Promise<UmbDataSourceErrorResponse>}
	 * @memberof UmbClipboardDetailLocalStorageDataSource
	 */
	async delete(unique: string): Promise<UmbDataSourceErrorResponse> {
		if (!unique) throw new Error('Unique is missing');
		const entries = this.#getEntriesFromLocalStorage();
		const updatedEntries = entries.filter((x) => x.unique !== unique);
		localStorage.setItem(UMB_CLIPBOARD_LOCALSTORAGE_KEY, JSON.stringify(updatedEntries));
		return {};
	}

	#getEntriesFromLocalStorage(): Array<UmbClipboardEntry> {
		const entries = localStorage.getItem(UMB_CLIPBOARD_LOCALSTORAGE_KEY);
		if (entries) {
			return JSON.parse(entries);
		}
		return [];
	}

	#getEntryFromLocalStorage(unique: string): UmbClipboardEntry | undefined {
		const entries = this.#getEntriesFromLocalStorage();
		return entries.find((x) => x.unique === unique);
	}
}
