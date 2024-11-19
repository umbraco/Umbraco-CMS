import type { UmbClipboardEntry } from '../../types.js';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbClipboardCollectionFilterModel } from '../types.js';

const UMB_CLIPBOARD_LOCALSTORAGE_KEY = 'umb:clipboard';

export class UmbClipboardCollectionLocalStorageDataSource implements UmbCollectionDataSource<UmbClipboardEntry> {
	async getCollection(filter: UmbClipboardCollectionFilterModel) {
		const items = this.#getEntriesFromLocalStorage();
		const total = items.length;
		return { data: { items, total } };
	}

	#getEntriesFromLocalStorage(): Array<UmbClipboardEntry> {
		const entries = localStorage.getItem(UMB_CLIPBOARD_LOCALSTORAGE_KEY);
		if (entries) {
			return JSON.parse(entries);
		}
		return [];
	}
}
