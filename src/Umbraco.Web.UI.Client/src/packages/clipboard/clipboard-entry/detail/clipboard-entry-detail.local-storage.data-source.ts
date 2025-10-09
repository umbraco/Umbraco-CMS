import type { UmbClipboardEntryDetailModel } from '../types.js';
import { UmbClipboardLocalStorageManager } from '../../clipboard-local-storage.manager.js';
import { UMB_CLIPBOARD_ENTRY_ENTITY_TYPE } from '../entity.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type {
	UmbDataSourceErrorResponse,
	UmbDataSourceResponse,
	UmbDetailDataSource,
} from '@umbraco-cms/backoffice/repository';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbError } from '@umbraco-cms/backoffice/resources';

/**
 * Manage clipboard entries in local storage
 * @export
 * @class UmbClipboardEntryDetailLocalStorageDataSource
 * @implements {UmbDetailDataSource<UmbClipboardEntryDetailModel>}
 */
export class UmbClipboardEntryDetailLocalStorageDataSource
	extends UmbControllerBase
	implements UmbDetailDataSource<UmbClipboardEntryDetailModel>
{
	#localStorageManager = new UmbClipboardLocalStorageManager(this);

	/**
	 * Scaffold a new clipboard entry
	 * @param {Partial<UmbClipboardEntryDetailModel>} [preset]
	 * @returns {*}
	 * @memberof UmbClipboardEntryDetailLocalStorageDataSource
	 */
	async createScaffold(preset: Partial<UmbClipboardEntryDetailModel> = {}) {
		const data: UmbClipboardEntryDetailModel = {
			values: [],
			entityType: UMB_CLIPBOARD_ENTRY_ENTITY_TYPE,
			icon: null,
			meta: {},
			name: null,
			unique: UmbId.new(),
			createDate: null,
			updateDate: null,
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
		if (!model) {
			return {
				error: new UmbError('Clipboard entry is missing'),
			};
		}

		// check if entry already exists
		const entry = await this.#localStorageManager.getEntry(model.unique);

		if (entry) {
			return {
				error: new UmbError('Clipboard entry already exists'),
			};
		}

		const now = new Date().toISOString();
		const newEntry: UmbClipboardEntryDetailModel = structuredClone(model);
		newEntry.createDate = now;
		newEntry.updateDate = now;

		const entriesResult = await this.#localStorageManager.getEntries();
		const updatedEntries = [...entriesResult.entries, newEntry];

		await this.#localStorageManager.setEntries(updatedEntries);

		return { data: newEntry };
	}

	/**
	 * Read a clipboard entry from local storage
	 * @param {string} unique
	 * @returns {*}  {Promise<UmbDataSourceResponse<UmbClipboardEntry>>}
	 * @memberof UmbClipboardEntryDetailLocalStorageDataSource
	 */
	async read(unique: string): Promise<UmbDataSourceResponse<UmbClipboardEntryDetailModel>> {
		if (!unique) {
			return {
				error: new UmbError('Unique is missing'),
			};
		}

		// check if entry exists
		const entry = await this.#localStorageManager.getEntry(unique);

		if (!entry) {
			return {
				error: new UmbError('Entry not found'),
			};
		}

		return { data: entry };
	}

	/**
	 * Update a clipboard entry in local storage
	 * @param {UmbClipboardEntryDetailModel} model
	 * @returns {*}  {Promise<UmbDataSourceResponse<UmbClipboardEntry>>}
	 * @memberof UmbClipboardEntryDetailLocalStorageDataSource
	 */
	async update(model: UmbClipboardEntryDetailModel): Promise<UmbDataSourceResponse<UmbClipboardEntryDetailModel>> {
		if (!model) {
			return {
				error: new UmbError('Clipboard entry is missing'),
			};
		}

		// check if entry exists so it can be updated
		const entry = await this.#localStorageManager.getEntry(model.unique);
		if (!entry) {
			return {
				error: new UmbError('Entry not found'),
			};
		}

		const entriesResult = await this.#localStorageManager.getEntries();

		const updatedEntries = entriesResult.entries.map((storedEntry) => {
			if (storedEntry.unique === model.unique) {
				const updatedEntry: UmbClipboardEntryDetailModel = structuredClone(model);
				updatedEntry.updateDate = new Date().toISOString();
				return updatedEntry;
			}

			return storedEntry;
		});

		await this.#localStorageManager.setEntries(updatedEntries);

		const updatedEntry = updatedEntries.find((x) => x.unique === model.unique);

		return { data: updatedEntry };
	}

	/**
	 * Delete a clipboard entry from local storage
	 * @param {string} unique
	 * @returns {*}  {Promise<UmbDataSourceErrorResponse>}
	 * @memberof UmbClipboardEntryDetailLocalStorageDataSource
	 */
	async delete(unique: string): Promise<UmbDataSourceErrorResponse> {
		if (!unique) {
			return {
				error: new UmbError('Unique is missing'),
			};
		}

		// check if entry exist so it can be deleted
		const entry = await this.#localStorageManager.getEntry(unique);

		if (!entry) {
			return {
				error: new UmbError('Entry not found'),
			};
		}

		const entriesResult = await this.#localStorageManager.getEntries();
		const updatedEntriesArray = entriesResult.entries.filter((x) => x.unique !== unique);

		await this.#localStorageManager.setEntries(updatedEntriesArray);
		return {};
	}
}
