import type { UmbUniqueTreeItemModel } from '../types.js';
import type { UmbTreeStore } from './tree-store.interface.js';
import { UmbStoreBase } from '@umbraco-cms/backoffice/store';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

// TODO: remove Unique from name when we have switched to uniques
/**
 * @export
 * @class UmbUniqueTreeStore
 * @extends {UmbStoreBase}
 * @description - Entity Tree Store
 */
export class UmbUniqueTreeStore
	extends UmbStoreBase<UmbUniqueTreeItemModel>
	implements UmbTreeStore<UmbUniqueTreeItemModel>
{
	constructor(host: UmbControllerHostElement, storeAlias: string) {
		super(host, storeAlias, new UmbArrayState<UmbUniqueTreeItemModel>([], (x) => x.unique));
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
		return this._data.asObservablePart((items) => items.filter((item) => uniques.includes(item.unique)));
	}
}
