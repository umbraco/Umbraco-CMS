import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbStoreBase } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UserResponseModel } from '@umbraco-cms/backoffice/backend-api';

export const UMB_USER_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbUserStore>('UmbUserStore');

/**
 * @export
 * @class UmbUserStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Users
 */
export class UmbUserStore extends UmbStoreBase {
	//#data = new UmbArrayState<UserResponseModel>([], (x) => x.id);

	constructor(host: UmbControllerHostElement) {
		super(host, UMB_USER_STORE_CONTEXT_TOKEN.toString(), new UmbArrayState<UserResponseModel>([], (x) => x.id));
	}

	/**
	 * Append a user to the store
	 * @param {UserResponseModel} user
	 * @memberof UmbUserStore
	 */
	append(user: UserResponseModel) {
		this._data.append([user]);
	}

	/**
	 * Get a user by id
	 * @param {id} string id.
	 * @memberof UmbUserStore
	 */
	byId(id: UserResponseModel['id']) {
		return this._data.asObservablePart((x) => x.find((y) => y.id === id));
	}

	/**
	 * Removes data-types in the store with the given uniques
	 * @param {string[]} uniques
	 * @memberof UmbUserStore
	 */
	remove(uniques: Array<UserResponseModel['id']>) {
		this._data.remove(uniques);
	}
}
