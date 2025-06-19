import { UmbClipboardLocalStorageManager } from '../../clipboard-local-storage.manager.js';
import type { UmbClipboardEntryItemModel } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbItemDataSource } from '@umbraco-cms/backoffice/repository';

/**
 * A local storage data source for Clipboard Entry items
 * @class UmbClipboardEntryItemServerDataSource
 * @implements {UmbItemServerDataSourceBase}
 */
export class UmbClipboardEntryItemLocalStorageDataSource
	extends UmbControllerBase
	implements UmbItemDataSource<UmbClipboardEntryItemModel>
{
	#localStorageManager = new UmbClipboardLocalStorageManager(this);

	/**
	 * Gets items from local storage
	 * @param {Array<string>} unique
	 * @memberof UmbClipboardEntryItemLocalStorageDataSource
	 */
	async getItems(unique: Array<string>) {
		const { entries } = await this.#localStorageManager.getEntries();
		const items = entries
			.filter((entry) => unique.includes(entry.unique))
			.map((entry) => {
				const item: UmbClipboardEntryItemModel = {
					entityType: entry.entityType,
					unique: entry.unique,
					name: entry.name,
					icon: entry.icon,
					meta: entry.meta,
					createDate: entry.createDate,
					updateDate: entry.updateDate,
				};
				return item;
			});
		return { data: items };
	}
}
