import { UmbDetailStore } from './detail-store.interface.js';
import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbStoreBase } from '@umbraco-cms/backoffice/store';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

/**
 * @export
 * @class UmbDetailStoreBase
 * @extends {UmbStoreBase}
 * @description - Data Store for Data Type items
 */

export abstract class UmbDetailStoreBase<T extends { unique: string }>
	extends UmbStoreBase<T>
	implements UmbDetailStore<T>
{
	/**
	 * Creates an instance of UmbDetailStoreBase.
	 * @param {UmbControllerHost} host
	 * @memberof UmbDetailStoreBase
	 */
	constructor(host: UmbControllerHost, storeAlias: string) {
		super(host, storeAlias, new UmbArrayState<T>([], (x) => x.unique));
	}

	/**
	 * Retrieve a detail model from the store
	 * @param {unique} string unique
	 * @memberof UmbDetailStoreBase
	 */
	byUnique(unique: string) {
		return this._data.asObservablePart((x) => x.find((y) => y.unique === unique));
	}
}
