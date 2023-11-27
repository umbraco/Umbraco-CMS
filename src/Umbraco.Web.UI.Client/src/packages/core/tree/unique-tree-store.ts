import { type UmbTreeStore } from './tree-store.interface.js';
import { UmbStoreBase } from '@umbraco-cms/backoffice/store';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

// temp model until cleanup is in place
interface TreeItemModel {
	unique: string;
	parentUnique: string | null;
}

// TODO: remove Unique from name when we have switched to uniques
/**
 * @export
 * @class UmbUniqueTreeStore
 * @extends {UmbStoreBase}
 * @description - Entity Tree Store
 */
export class UmbUniqueTreeStore extends UmbStoreBase<TreeItemModel> implements UmbTreeStore<TreeItemModel> {
	constructor(host: UmbControllerHostElement, storeAlias: string) {
		super(host, storeAlias, new UmbArrayState<TreeItemModel>([], (x) => x.unique));
	}

	/**
	 * An observable to observe the root items
	 * @memberof UmbUniqueTreeStore
	 */
	rootItems = this._data.asObservablePart((items) => items.filter((item) => item.parentUnique === null));

	/**
	 * Returns an observable to observe the children of a given parent
	 * @param {(string | null)} parentUnique
	 * @return {*}
	 * @memberof UmbUniqueTreeStore
	 */
	childrenOf(parentUnique: string | null) {
		return this._data.asObservablePart((items) => items.filter((item) => item.parentUnique === parentUnique));
	}

	/**
	 * Returns an observable to observe the items with the given uniques
	 * @param {Array<string>} uniques
	 * @return {*}
	 * @memberof UmbUniqueTreeStore
	 */
	items(uniques: Array<string | null>) {
		return this._data.asObservablePart((items) => items.filter((item) => uniques.includes(item.unique ?? '')));
	}
}
