import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	CreateUserDataRequestModel,
	UpdateUserDataRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { UserDataService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the current user data that fetches data from the server
 * @class UmbCurrentUserDataServerDataSource
 */
export class UmbCurrentUserDataServerDataSource {
	#host;

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Get all user data for the current user
	 * @param {{ groups?: Array<string>; identifiers?: Array<string>; skip?: number; take?: number }} [args]
	 * @memberof UmbCurrentUserDataServerDataSource
	 */
	getUserData(args?: { groups?: Array<string>; identifiers?: Array<string>; skip?: number; take?: number }) {
		return tryExecute(this.#host, UserDataService.getUserData({ query: args }));
	}

	/**
	 * Get user data by id
	 * @param {string} id
	 * @memberof UmbCurrentUserDataServerDataSource
	 */
	getUserDataById(id: string) {
		return tryExecute(this.#host, UserDataService.getUserDataById({ path: { id } }));
	}

	/**
	 * Create user data
	 * @param {CreateUserDataRequestModel} body
	 * @memberof UmbCurrentUserDataServerDataSource
	 */
	createUserData(body: CreateUserDataRequestModel) {
		return tryExecute(this.#host, UserDataService.postUserData({ body }));
	}

	/**
	 * Update user data
	 * @param {UpdateUserDataRequestModel} body
	 * @memberof UmbCurrentUserDataServerDataSource
	 */
	updateUserData(body: UpdateUserDataRequestModel) {
		return tryExecute(this.#host, UserDataService.putUserData({ body }));
	}

	/**
	 * Delete user data by id
	 * @param {string} id
	 * @memberof UmbCurrentUserDataServerDataSource
	 */
	deleteUserData(id: string) {
		return tryExecute(this.#host, UserDataService.deleteUserDataById({ path: { id } }));
	}
}
