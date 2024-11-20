import type { UmbClipboardEntry } from '../../types.js';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/collection';
import type { UmbClipboardCollectionFilterModel } from '../types.js';
import { UmbClipboardLocalStorageManager } from '../../clipboard-local-storage.manager.js';

export class UmbClipboardCollectionLocalStorageDataSource implements UmbCollectionDataSource<UmbClipboardEntry> {
	#localStorageManager = new UmbClipboardLocalStorageManager();

	async getCollection(filter: UmbClipboardCollectionFilterModel) {
		const { entries, total } = this.#localStorageManager.getEntries();
		return { data: { items: entries, total } };
	}
}
