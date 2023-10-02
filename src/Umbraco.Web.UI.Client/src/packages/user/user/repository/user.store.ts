import { type UmbUserDetail } from '../index.js';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbStoreBase } from '@umbraco-cms/backoffice/store';
import { type UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export const UMB_USER_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbUserStore>('UmbUserStore');

/**
 * @export
 * @class UmbUserStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Users
 */
export class UmbUserStore extends UmbStoreBase {
	//#data = new UmbArrayState<UmbUserDetail>([], (x) => x.id);

	constructor(host: UmbControllerHostElement) {
		super(host, UMB_USER_STORE_CONTEXT_TOKEN.toString(), new UmbArrayState<UmbUserDetail>([], (x) => x.id));
	}

	/**
	 * Append a user to the store
	 * @param {UmbUserDetail} user
	 * @memberof UmbUserStore
	 */
	append(user: UmbUserDetail) {
		this._data.append([user]);
	}

	/**
	 * Get a user by id
	 * @param {id} string id.
	 * @memberof UmbUserStore
	 */
	byId(id: UmbUserDetail['id']) {
		return this._data.asObservablePart((x) => x.find((y) => y.id === id));
	}

	/**
	 * Removes data-types in the store with the given uniques
	 * @param {string[]} uniques
	 * @memberof UmbUserStore
	 */
	remove(uniques: Array<UmbUserDetail['id']>) {
		this._data.remove(uniques);
	}
}
