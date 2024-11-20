import type { UmbClipboardEntryDetailModel } from './clipboard-entry/index.js';

const UMB_CLIPBOARD_LOCALSTORAGE_KEY = 'umb:clipboard';

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
}
