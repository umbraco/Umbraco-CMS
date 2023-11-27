import { UmbStore } from './store.interface.js';
import { UmbStoreRemoveEvent } from './events/store-remove.event.js';
import { UmbStoreUpdateEvent } from './events/store-update.event.js';
import { UmbStoreAppendEvent } from './events/store-append.event.js';
import { UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { type UmbApi } from '@umbraco-cms/backoffice/extension-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

export class UmbStoreBase<StoreItemType = any> extends EventTarget implements UmbStore<StoreItemType>, UmbApi {
	#provider;

	protected _host: UmbControllerHost;
	protected _data: UmbArrayState<StoreItemType>;

	public readonly storeAlias: string;

	constructor(_host: UmbControllerHost, storeAlias: string, data: UmbArrayState<StoreItemType>) {
		super();
		this._host = _host;
		this.storeAlias = storeAlias;
		this._data = data;

		this.#provider = new UmbContextProviderController(_host, storeAlias, this);
	}

	/**
	 * Append an item to the store
	 * @param {StoreItemType} item
	 * @memberof UmbStoreBase
	 */
	append(item: StoreItemType) {
		this._data.append([item]);
		const unique = this._data.getUnique(item) as string;
		this.dispatchEvent(new UmbStoreAppendEvent([unique]));
	}

	/**
	 * Appends multiple items to the store
	 * @param {Array<StoreItemType>} items
	 * @memberof UmbStoreBase
	 */
	appendItems(items: Array<StoreItemType>) {
		this._data.append(items);
		const uniques = items.map((item) => this._data.getUnique(item)) as Array<string>;
		this.dispatchEvent(new UmbStoreAppendEvent(uniques));
	}

	/**
	 * Updates an item in the store
	 * @param {string} unique
	 * @param {Partial<StoreItemType>} data
	 * @memberof UmbStoreBase
	 */
	updateItem(unique: string, data: Partial<StoreItemType>) {
		this._data.updateOne(unique, data);
		this.dispatchEvent(new UmbStoreUpdateEvent([unique]));
	}

	/**
	 * Removes an item from the store
	 * @param {string} unique
	 * @memberof UmbStoreBase
	 */
	removeItem(unique: string) {
		this._data.removeOne(unique);
		this.dispatchEvent(new UmbStoreRemoveEvent([unique]));
	}

	/**
	 * Removes multiple items in the store with the given uniques
	 * @param {string[]} uniques
	 * @memberof UmbStoreBase
	 */
	removeItems(uniques: Array<string>) {
		this._data.remove(uniques);
		this.dispatchEvent(new UmbStoreRemoveEvent(uniques));
	}

	/**
	 * Returns an array of items with the given uniques
	 * @param {string[]} uniques
	 * @returns {Array<StoreItemType>}
	 * @memberof UmbStoreBase
	 */
	getItems(uniques: Array<string>) {
		return this._data.getValue().filter((item) => uniques.includes(this._data.getUnique(item)));
	}

	/**
	 * Returns an observable of the entire store
	 * @memberof UmbStoreBase
	 */
	all() {
		return this._data.asObservable();
	}

	destroy() {
		this.#provider.destroy();
	}
}
