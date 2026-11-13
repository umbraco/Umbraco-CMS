import { UmbCurrentUserDataServerDataSource } from './current-user-data.server.data-source.js';
import type {
	CreateUserDataRequestModel,
	UpdateUserDataRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';

/**
 * A repository for the current user data
 * @class UmbCurrentUserDataRepository
 * @augments {UmbRepositoryBase}
 */
export class UmbCurrentUserDataRepository extends UmbRepositoryBase {
	#dataSource = new UmbCurrentUserDataServerDataSource(this._host);

	constructor(host: UmbControllerHost) {
		super(host);
	}

	/**
	 * Get all user data for the current user
	 * @param {{ groups?: Array<string>; identifiers?: Array<string>; skip?: number; take?: number }} [args]
	 * @memberof UmbCurrentUserDataRepository
	 */
	async getUserData(args?: { groups?: Array<string>; identifiers?: Array<string>; skip?: number; take?: number }) {
		return this.#dataSource.getUserData(args);
	}

	/**
	 * Get user data by id
	 * @param {string} id
	 * @memberof UmbCurrentUserDataRepository
	 */
	async getUserDataById(id: string) {
		return this.#dataSource.getUserDataById(id);
	}

	/**
	 * Create user data
	 * @param {CreateUserDataRequestModel} body
	 * @memberof UmbCurrentUserDataRepository
	 */
	async createUserData(body: CreateUserDataRequestModel) {
		return this.#dataSource.createUserData(body);
	}

	/**
	 * Update user data
	 * @param {UpdateUserDataRequestModel} body
	 * @memberof UmbCurrentUserDataRepository
	 */
	async updateUserData(body: UpdateUserDataRequestModel) {
		return this.#dataSource.updateUserData(body);
	}

	/**
	 * Delete user data by id
	 * @param {string} id
	 * @memberof UmbCurrentUserDataRepository
	 */
	async deleteUserData(id: string) {
		return this.#dataSource.deleteUserData(id);
	}
}

export default UmbCurrentUserDataRepository;
