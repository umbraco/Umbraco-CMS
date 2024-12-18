import type { UmbClipboardEntryDetailModel } from './clipboard-entry/index.js';

const UMB_CLIPBOARD_LOCALSTORAGE_KEY = 'umb:clipboard';

interface UmbClipboardLocalStorageFilterModel {
	entry?: {
		types: Array<string>;
	};
	skip?: number;
	take?: number;
}

// keep internal
export class UmbClipboardLocalStorageManager {
	// Gets all entries from local storage
	getEntries(): {
		entries: Array<UmbClipboardEntryDetailModel>;
		total: number;
	} {
		const localStorageItem = localStorage.getItem(UMB_CLIPBOARD_LOCALSTORAGE_KEY);
		const entries = localStorageItem ? JSON.parse(localStorageItem) : [];
		const total = entries.length;
		return { entries, total };
	}

	// Gets a single entry from local storage
	getEntry(unique: string): {
		entry: UmbClipboardEntryDetailModel | undefined;
		entries: Array<UmbClipboardEntryDetailModel>;
	} {
		const { entries } = this.getEntries();
		const entry = entries.find((x) => x.unique === unique);
		return { entries, entry };
	}

	// Sets all entries in local storage
	setEntries(entries: Array<UmbClipboardEntryDetailModel>) {
		localStorage.setItem(UMB_CLIPBOARD_LOCALSTORAGE_KEY, JSON.stringify(entries));
	}

	// gets a filtered list of entries
	filter(filter: UmbClipboardLocalStorageFilterModel) {
		const { entries } = this.getEntries();
		const filteredEntries = this.#filterEntries(entries, filter);
		const total = filteredEntries.length;
		const skip = filter.skip || 0;
		const take = filter.take || total;
		const pagedEntries = filteredEntries.slice(skip, skip + take);
		return { entries: pagedEntries, total };
	}

	#filterEntries(entries: Array<UmbClipboardEntryDetailModel>, filter: UmbClipboardLocalStorageFilterModel) {
		return entries.filter((entry) => {
			if (filter.entry?.types) {
				return filter.entry.types.includes(entry.type);
			}
			return true;
		});
	}
}
