import { EntityTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { ArrayState, partialUpdateFrozenArray } from '@umbraco-cms/backoffice/observable-api';
import { UmbStoreBase, UmbTreeStore } from '@umbraco-cms/backoffice/store';

/**
 * @export
 * @class UmbEntityTreeStore
 * @extends {UmbStoreBase}
 * @description - General Tree Data Store
 */
export class UmbEntityTreeStore extends UmbStoreBase implements UmbTreeStore<EntityTreeItemResponseModel> {
	#data = new ArrayState<EntityTreeItemResponseModel>([], (x) => x.key);

	/**
	 * Appends items to the store
	 * @param {Array<EntityTreeItemResponseModel>} items
	 * @memberof UmbEntityTreeStore
	 */
	appendItems(items: Array<EntityTreeItemResponseModel>) {
		this.#data.append(items);
	}

	/**
	 * Updates an item in the store
	 * @param {string} key
	 * @param {Partial<EntityTreeItemResponseModel>} data
	 * @memberof UmbEntityTreeStore
	 */
	updateItem(key: string, data: Partial<EntityTreeItemResponseModel>) {
		this.#data.next(partialUpdateFrozenArray(this.#data.getValue(), data, (entry) => entry.key === key));
	}

	/**
	 * Removes an item from the store
	 * @param {string} key
	 * @memberof UmbEntityTreeStore
	 */
	removeItem(key: string) {
		this.#data.removeOne(key);
	}

	/**
	 * An observable to observe the root items
	 * @memberof UmbEntityTreeStore
	 */
	rootItems = this.#data.getObservablePart((items) => items.filter((item) => item.parentKey === null));

	/**
	 * Returns an observable to observe the children of a given parent
	 * @param {(string | null)} parentKey
	 * @return {*}
	 * @memberof UmbEntityTreeStore
	 */
	childrenOf(parentKey: string | null) {
		return this.#data.getObservablePart((items) => items.filter((item) => item.parentKey === parentKey));
	}

	/**
	 * Returns an observable to observe the items with the given keys
	 * @param {Array<string>} keys
	 * @return {*}
	 * @memberof UmbEntityTreeStore
	 */
	items(keys: Array<string>) {
		return this.#data.getObservablePart((items) => items.filter((item) => keys.includes(item.key ?? '')));
	}
}
