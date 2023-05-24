import { UmbStore } from './store.interface.js';
import { UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

// TODO: Make a Store interface?
export class UmbStoreBase<StoreItemType = any> implements UmbStore<StoreItemType> {
	protected _host: UmbControllerHostElement;
	protected _data: UmbArrayState<StoreItemType>;

	public readonly storeAlias: string;

	constructor(_host: UmbControllerHostElement, storeAlias: string, data: UmbArrayState<StoreItemType>) {
		this._host = _host;
		this.storeAlias = storeAlias;
		this._data = data;

		new UmbContextProviderController(_host, storeAlias, this);
	}

	/**
	 * Appends items to the store
	 * @param {Array<StoreItemType>} items
	 * @memberof UmbEntityTreeStore
	 */
	appendItems(items: Array<StoreItemType>) {
		this._data.append(items);
	}

	/**
	 * Updates an item in the store
	 * @param {string} id
	 * @param {Partial<StoreItemType>} data
	 * @memberof UmbEntityTreeStore
	 */
	updateItem(unique: string, data: Partial<StoreItemType>) {
		this._data.updateOne(unique, data);
	}

	/**
	 * Removes an item from the store
	 * @param {string} id
	 * @memberof UmbEntityTreeStore
	 */
	removeItem(unique: string) {
		this._data.removeOne(unique);
	}
}
