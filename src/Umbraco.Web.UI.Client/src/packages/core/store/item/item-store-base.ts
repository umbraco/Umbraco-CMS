import { UmbStoreBase } from '../store-base.js';
import type { UmbItemStore } from './item-store.interface.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

/**
 * @class UmbItemStoreBase
 * @augments {UmbStoreBase}
 * @description - Data Store for items with a unique property
 */

export abstract class UmbItemStoreBase<T extends { unique: string }>
	extends UmbStoreBase<T>
	implements UmbItemStore<T>
{
	/**
	 * Creates an instance of UmbItemStoreBase.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @param storeAlias
	 * @memberof UmbItemStoreBase
	 */
	constructor(host: UmbControllerHost, storeAlias: string) {
		super(host, storeAlias, new UmbArrayState<T>([], (x) => x.unique));
	}

	items(uniques: Array<string>) {
		return this._data.asObservablePart((items) => items.filter((item) => uniques.includes(item.unique)));
	}
}
