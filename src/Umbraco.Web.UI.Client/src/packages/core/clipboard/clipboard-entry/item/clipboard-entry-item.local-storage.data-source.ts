import { type UmbDataSourceResponse, type UmbItemDataSource } from '@umbraco-cms/backoffice/repository';
import { UmbClipboardLocalStorageManager } from '../../clipboard-local-storage.manager.js';
import type { UmbClipboardEntry } from '../types.js';

/**
 * A local storage data source for Clipboard Entry items
 * @class UmbClipboardEntryItemServerDataSource
 * @implements {UmbItemServerDataSourceBase}
 */
export class UmbClipboardEntryItemLocalStorageDataSource implements UmbItemDataSource<UmbClipboardEntry> {
	#localStorageManager = new UmbClipboardLocalStorageManager();

	/**
	 * Gets items from local storage
	 * @param {Array<string>} unique
	 * @return {*}
	 * @memberof UmbClipboardEntryItemLocalStorageDataSource
	 */
	async getItems(unique: Array<string>): Promise<UmbDataSourceResponse<Array<UmbClipboardEntry>>> {
		const { entries } = this.#localStorageManager.getEntries();
		const items = entries.filter((x) => unique.includes(x.unique));
		return { data: items };
	}
}
