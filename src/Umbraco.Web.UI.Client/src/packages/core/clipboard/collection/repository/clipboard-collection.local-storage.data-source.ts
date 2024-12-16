import type { UmbClipboardCollectionFilterModel } from '../types.js';
import { UmbClipboardLocalStorageManager } from '../../clipboard-local-storage.manager.js';
import type { UmbClipboardEntryDetailModel } from '../../clipboard-entry/index.js';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/collection';

export class UmbClipboardCollectionLocalStorageDataSource
	implements UmbCollectionDataSource<UmbClipboardEntryDetailModel>
{
	#localStorageManager = new UmbClipboardLocalStorageManager();

	async getCollection(filter: UmbClipboardCollectionFilterModel) {
		const { entries, total } = this.#localStorageManager.filter(filter);
		return { data: { items: entries, total } };
	}
}
