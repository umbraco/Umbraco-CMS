import type { UmbClipboardCollectionFilterModel } from '../types.js';
import { UmbClipboardLocalStorageManager } from '../../clipboard-local-storage.manager.js';
import type { UmbClipboardEntryDetailModel } from '../../clipboard-entry/index.js';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/collection';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

export class UmbClipboardCollectionLocalStorageDataSource
	extends UmbControllerBase
	implements UmbCollectionDataSource<UmbClipboardEntryDetailModel>
{
	#localStorageManager = new UmbClipboardLocalStorageManager(this);

	async getCollection(filter: UmbClipboardCollectionFilterModel) {
		const { entries, total } = await this.#localStorageManager.filter(filter);

		if (!filter.asyncFilter) {
			return { data: { items: entries, total } };
		}

		const results = await Promise.all(entries.map(filter.asyncFilter));
		const filtered = entries.filter((_, i) => results[i]);
		return { data: { items: filtered, total: filtered.length } };
	}
}
