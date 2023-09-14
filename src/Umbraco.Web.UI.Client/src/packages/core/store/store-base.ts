import { UmbStore } from './store.interface.js';
import { UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

// TODO: Make a Store interface?
export class UmbStoreBase<StoreItemType = any> implements UmbStore<StoreItemType> {
	protected _host: UmbControllerHost;
	protected _data: UmbArrayState<StoreItemType>;

	public readonly storeAlias: string;

	constructor(_host: UmbControllerHost, storeAlias: string, data: UmbArrayState<StoreItemType>) {
		this._host = _host;
		this.storeAlias = storeAlias;
		this._data = data;

		new UmbContextProviderController(_host, storeAlias, this);
	}

	/**
	 * Append an item to the store
	 * @param {StoreItemType} item
	 * @memberof UmbStoreBase
	 */
	append(item: StoreItemType) {
		this._data.append([item]);
	}

	/**
	 * Appends multiple items to the store
	 * @param {Array<StoreItemType>} items
	 * @memberof UmbStoreBase
	 */
	appendItems(items: Array<StoreItemType>) {
		this._data.append(items);
	}

	/**
	 * Updates an item in the store
	 * @param {string} unique
	 * @param {Partial<StoreItemType>} data
	 * @memberof UmbStoreBase
	 */
	updateItem(unique: string, data: Partial<StoreItemType>) {
		this._data.updateOne(unique, data);
	}

	/**
	 * Removes an item from the store
	 * @param {string} unique
	 * @memberof UmbStoreBase
	 */
	removeItem(unique: string) {
		this._data.removeOne(unique);
	}

	/**
	 * Removes multiple items in the store with the given uniques
	 * @param {string[]} uniques
	 * @memberof UmbStoreBase
	 */
	removeItems(uniques: Array<string>) {
		this._data.remove(uniques);
	}
}
