import type { UmbClipboardEntryDetailModel } from '../types.js';
import { UmbClipboardLocalStorageManager } from '../../clipboard-local-storage.manager.js';
import { UMB_CLIPBOARD_ENTRY_ENTITY_TYPE } from '../entity.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type {
	UmbDataSourceErrorResponse,
	UmbDataSourceResponse,
	UmbDetailDataSource,
} from '@umbraco-cms/backoffice/repository';

/**
 * Manage clipboard entries in local storage
 * @export
 * @class UmbClipboardEntryDetailLocalStorageDataSource
 * @implements {UmbDetailDataSource<UmbClipboardEntryDetailModel>}
 */
export class UmbClipboardEntryDetailLocalStorageDataSource
	implements UmbDetailDataSource<UmbClipboardEntryDetailModel>
{
	#localStorageManager = new UmbClipboardLocalStorageManager();

	/**
	 * Scaffold a new clipboard entry
	 * @param {Partial<UmbClipboardEntryDetailModel>} [preset]
	 * @returns {*}
	 * @memberof UmbClipboardEntryDetailLocalStorageDataSource
	 */
	async createScaffold(preset: Partial<UmbClipboardEntryDetailModel> = {}) {
		const data: UmbClipboardEntryDetailModel = {
			data: [],
			entityType: UMB_CLIPBOARD_ENTRY_ENTITY_TYPE,
			icons: [],
			meta: {},
			name: '',
			type: '',
			unique: UmbId.new(),
			...preset,
		};

		return { data };
	}

	/**
	 * Create a new clipboard entry in local storage
	 * @param {UmbClipboardEntryDetailModel} model
	 * @returns {*}  {Promise<UmbDataSourceResponse<UmbClipboardEntry>>}
	 * @memberof UmbClipboardEntryDetailLocalStorageDataSource
	 */
	async create(model: UmbClipboardEntryDetailModel): Promise<UmbDataSourceResponse<UmbClipboardEntryDetailModel>> {
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
	 * @returns {*}  {Promise<UmbDataSourceResponse<UmbClipboardEntry>>}
	 * @memberof UmbClipboardEntryDetailLocalStorageDataSource
	 */
	async read(unique: string): Promise<UmbDataSourceResponse<UmbClipboardEntryDetailModel>> {
		if (!unique) return { error: new Error('Unique is missing') };

		// check if entry exists
		const { entry } = this.#localStorageManager.getEntry(unique);
		if (!entry) return { error: new Error('Clipboard entry not found') };

		return { data: entry };
	}

	/**
	 * Update a clipboard entry in local storage
	 * @param {UmbClipboardEntryDetailModel} model
	 * @returns {*}  {Promise<UmbDataSourceResponse<UmbClipboardEntry>>}
	 * @memberof UmbClipboardEntryDetailLocalStorageDataSource
	 */
	async update(model: UmbClipboardEntryDetailModel): Promise<UmbDataSourceResponse<UmbClipboardEntryDetailModel>> {
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
	 * @returns {*}  {Promise<UmbDataSourceErrorResponse>}
	 * @memberof UmbClipboardEntryDetailLocalStorageDataSource
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
