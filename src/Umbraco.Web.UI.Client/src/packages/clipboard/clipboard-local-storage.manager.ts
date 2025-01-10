import type { UmbClipboardEntryDetailModel } from './clipboard-entry/index.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';

const UMB_CLIPBOARD_LOCAL_STORAGE_KEY_PREFIX = 'umb:clipboard';

interface UmbClipboardLocalStorageFilterModel {
	types?: Array<string>;
	skip?: number;
	take?: number;
}

// keep internal
export class UmbClipboardLocalStorageManager extends UmbControllerBase {
	#currentUserUnique?: string;

	constructor(host: UmbControllerHost) {
		super(host);
	}

	// Gets all entries from local storage
	async getEntries(): Promise<{
		entries: Array<UmbClipboardEntryDetailModel>;
		total: number;
	}> {
		const localStorageKey = await this.#requestLocalStorageKey();
		const localStorageItem = localStorage.getItem(localStorageKey);
		const entries = localStorageItem ? JSON.parse(localStorageItem) : [];
		const total = entries.length;
		return { entries, total };
	}

	// Gets a single entry from local storage
	async getEntry(unique: string): Promise<UmbClipboardEntryDetailModel | undefined> {
		const { entries } = await this.getEntries();
		return entries.find((x) => x.unique === unique);
	}

	// Sets all entries in local storage
	async setEntries(entries: Array<UmbClipboardEntryDetailModel>) {
		const currentUserUnique = await this.#requestCurrentUserUnique();

		if (!currentUserUnique) {
			throw new Error('Could not get current user unique');
		}

		const localStorageKey = await this.#requestLocalStorageKey();

		localStorage.setItem(localStorageKey, JSON.stringify(entries));
	}

	// gets a filtered list of entries
	async filter(filter: UmbClipboardLocalStorageFilterModel) {
		const { entries } = await this.getEntries();
		const filteredEntries = this.#filterEntries(entries, filter);
		const total = filteredEntries.length;
		const skip = filter.skip || 0;
		const take = filter.take || total;
		const pagedEntries = filteredEntries.slice(skip, skip + take);
		return { entries: pagedEntries, total };
	}

	#filterEntries(entries: Array<UmbClipboardEntryDetailModel>, filter: UmbClipboardLocalStorageFilterModel) {
		return entries.filter((entry) => {
			if (filter.types?.length) {
				const valueTypes = entry.values.map((x) => x.type);
				return filter.types.some((type) => valueTypes.includes(type));
			}
			return true;
		});
	}

	async #requestLocalStorageKey() {
		const currentUserUnique = await this.#requestCurrentUserUnique();
		return `${UMB_CLIPBOARD_LOCAL_STORAGE_KEY_PREFIX}:${currentUserUnique}`;
	}

	async #requestCurrentUserUnique() {
		if (this.#currentUserUnique) {
			return this.#currentUserUnique;
		}

		const context = await this.getContext(UMB_CURRENT_USER_CONTEXT);
		this.#currentUserUnique = context.getUnique();
		return this.#currentUserUnique;
	}
}
