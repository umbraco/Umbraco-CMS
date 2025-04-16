import { UmbStoreBase } from '../store-base.js';
import type { UmbDetailStore } from './detail-store.interface.js';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

/**
 * @class UmbDetailStoreBase
 * @augments {UmbStoreBase}
 * @description - Data Store for Data Type items
 */

export abstract class UmbDetailStoreBase<T extends UmbEntityModel>
	extends UmbStoreBase<T>
	implements UmbDetailStore<T>
{
	/**
	 * Creates an instance of UmbDetailStoreBase.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @param storeAlias - The alias of the store
	 * @memberof UmbDetailStoreBase
	 */
	constructor(host: UmbControllerHost, storeAlias: string) {
		super(host, storeAlias, new UmbArrayState<T>([], (x) => x.unique));
	}

	/**
	 * Retrieve a detail model from the store
	 * @param {unique} string unique identifier
	 * @param unique
	 * @returns {Observable<T>}
	 * @memberof UmbDetailStoreBase
	 */
	byUnique(unique: string) {
		return this._data.asObservablePart((x) => x.find((y) => y.unique === unique));
	}
}
