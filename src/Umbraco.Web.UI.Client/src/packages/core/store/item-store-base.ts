import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbItemStore} from '@umbraco-cms/backoffice/store';
import { UmbStoreBase } from '@umbraco-cms/backoffice/store';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

/**
 * @export
 * @class UmbItemStoreBase
 * @extends {UmbStoreBase}
 * @description - Data Store for items with a unique property
 */

export abstract class UmbItemStoreBase<T extends { unique: string }>
	extends UmbStoreBase<T>
	implements UmbItemStore<T>
{
	/**
	 * Creates an instance of UmbItemStoreBase.
	 * @param {UmbControllerHost} host
	 * @memberof UmbItemStoreBase
	 */
	constructor(host: UmbControllerHost, storeAlias: string) {
		super(host, storeAlias, new UmbArrayState<T>([], (x) => x.unique));
	}

	items(uniques: Array<string>) {
		return this._data.asObservablePart((items) => items.filter((item) => uniques.includes(item.unique)));
	}
}
