import type { UmbTreeItemModel } from '../types.js';
import type { UmbTreeStore } from './tree-store.interface.js';
import { UmbStoreBase } from '@umbraco-cms/backoffice/store';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

// TODO: remove Unique from name when we have switched to uniques
/**
 * @class UmbUniqueTreeStore
 * @augments {UmbStoreBase}
 * @description - Entity Tree Store
 */
export class UmbUniqueTreeStore extends UmbStoreBase<UmbTreeItemModel> implements UmbTreeStore<UmbTreeItemModel> {
	constructor(host: UmbControllerHost, storeAlias: string) {
		super(host, storeAlias, new UmbArrayState<UmbTreeItemModel>([], (x) => x.unique));
	}

	/**
	 * An observable to observe the root items
	 * @memberof UmbUniqueTreeStore
	 */
	rootItems = this._data.asObservablePart((items) => items.filter((item) => item.parent.unique === null));

	/**
	 * Returns an observable to observe the children of a given parent
	 * @param {(string | null)} parentUnique
	 * @returns {*}
	 * @memberof UmbUniqueTreeStore
	 */
	childrenOf(parentUnique: string | null) {
		return this._data.asObservablePart((items) => items.filter((item) => item.parent.unique === parentUnique));
	}
}
