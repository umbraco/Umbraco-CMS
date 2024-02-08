import { UmbStoreBase } from './store-base.js';
import type { ItemResponseModelBaseModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

/**
 * @export
 * @class UmbDataTypeItemStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Data Type items
 */

export class UmbEntityItemStore<T extends ItemResponseModelBaseModel> extends UmbStoreBase<T> {
	/**
	 * Creates an instance of UmbEntityItemStore.
	 * @param {UmbControllerHost} host
	 * @memberof UmbDataTypeItemStore
	 */
	constructor(host: UmbControllerHost, storeAlias: string) {
		super(host, storeAlias, new UmbArrayState<T>([], (x) => x.id));
	}

	items(ids: Array<string>) {
		return this._data.asObservablePart((items) => items.filter((item) => ids.includes(item.id)));
	}
}
