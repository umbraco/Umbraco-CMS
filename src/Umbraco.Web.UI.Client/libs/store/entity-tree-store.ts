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
	#data = new ArrayState<EntityTreeItemResponseModel>([], (x) => x.id);

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
	 * @param {string} id
	 * @param {Partial<EntityTreeItemResponseModel>} data
	 * @memberof UmbEntityTreeStore
	 */
	updateItem(id: string, data: Partial<EntityTreeItemResponseModel>) {
		this.#data.next(partialUpdateFrozenArray(this.#data.getValue(), data, (entry) => entry.id === id));
	}

	/**
	 * Removes an item from the store
	 * @param {string} id
	 * @memberof UmbEntityTreeStore
	 */
	removeItem(id: string) {
		this.#data.removeOne(id);
	}

	/**
	 * An observable to observe the root items
	 * @memberof UmbEntityTreeStore
	 */
	rootItems = this.#data.getObservablePart((items) => items.filter((item) => item.parentId === null));

	/**
	 * Returns an observable to observe the children of a given parent
	 * @param {(string | null)} parentId
	 * @return {*}
	 * @memberof UmbEntityTreeStore
	 */
	childrenOf(parentId: string | null) {
		return this.#data.getObservablePart((items) => items.filter((item) => item.parentId === parentId));
	}

	/**
	 * Returns an observable to observe the items with the given ids
	 * @param {Array<string>} ids
	 * @return {*}
	 * @memberof UmbEntityTreeStore
	 */
	items(ids: Array<string>) {
		return this.#data.getObservablePart((items) => items.filter((item) => ids.includes(item.id ?? '')));
	}
}
