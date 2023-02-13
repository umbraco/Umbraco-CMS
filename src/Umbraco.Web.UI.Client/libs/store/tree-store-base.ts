import { EntityTreeItemModel } from '@umbraco-cms/backend-api';
import { ArrayState, partialUpdateFrozenArray } from '@umbraco-cms/observable-api';
import { UmbStoreBase } from '@umbraco-cms/store';

/**
 * @export
 * @class UmbTreeStoreBase
 * @extends {UmbStoreBase}
 * @description - General Tree Data Store
 */
// TODO: consider if tree store could be turned into a general EntityTreeStore class?
export class UmbTreeStoreBase extends UmbStoreBase {
	#data = new ArrayState<EntityTreeItemModel>([], (x) => x.key);

	/**
	 * Appends items to the store
	 * @param {Array<EntityTreeItemModel>} items
	 * @memberof UmbTreeStoreBase
	 */
	appendItems(items: Array<EntityTreeItemModel>) {
		this.#data.append(items);
	}

	/**
	 * Updates an item in the store
	 * @param {string} key
	 * @param {Partial<EntityTreeItemModel>} data
	 * @memberof UmbTreeStoreBase
	 */
	updateItem(key: string, data: Partial<EntityTreeItemModel>) {
		this.#data.next(partialUpdateFrozenArray(this.#data.getValue(), data, (entry) => entry.key === key));
	}

	/**
	 * Removes an item from the store
	 * @param {string} key
	 * @memberof UmbTreeStoreBase
	 */
	removeItem(key: string) {
		this.#data.removeOne(key);
	}

	/**
	 * An observable to observe the root items
	 * @memberof UmbTreeStoreBase
	 */
	rootItems = this.#data.getObservablePart((items) => items.filter((item) => item.parentKey === null));

	/**
	 * Returns an observable to observe the children of a given parent
	 * @param {(string | null)} parentKey
	 * @return {*}
	 * @memberof UmbTreeStoreBase
	 */
	childrenOf(parentKey: string | null) {
		return this.#data.getObservablePart((items) => items.filter((item) => item.parentKey === parentKey));
	}

	/**
	 * Returns an observable to observe the items with the given keys
	 * @param {Array<string>} keys
	 * @return {*}
	 * @memberof UmbTreeStoreBase
	 */
	items(keys: Array<string>) {
		return this.#data.getObservablePart((items) => items.filter((item) => keys.includes(item.key ?? '')));
	}
}
