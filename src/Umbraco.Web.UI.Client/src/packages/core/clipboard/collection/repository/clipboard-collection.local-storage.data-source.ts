import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/collection';
import type { UmbClipboardCollectionFilterModel } from '../types.js';
import { UmbClipboardLocalStorageManager } from '../../clipboard-local-storage.manager.js';
import type { UmbClipboardEntry } from '../../clipboard-entry/index.js';

export class UmbClipboardCollectionLocalStorageDataSource implements UmbCollectionDataSource<UmbClipboardEntry> {
	#localStorageManager = new UmbClipboardLocalStorageManager();

	async getCollection(filter: UmbClipboardCollectionFilterModel) {
		const { entries, total } = this.#localStorageManager.getEntries();
		return { data: { items: entries, total } };
	}
}
