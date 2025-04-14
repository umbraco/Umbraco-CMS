import { UserService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

/**
 * A server data source for generating and assigning a new password for a user
 * @class UmbNewUserPasswordServerDataSource
 */
export class UmbNewUserPasswordServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbNewUserPasswordServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbNewUserPasswordServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Generate a new password for a user
	 * @param {string} unique
	 * @returns {*}
	 * @memberof UmbNewUserPasswordServerDataSource
	 */
	async newPassword(unique: string) {
		if (!unique) throw new Error('User unique is missing');

		return tryExecute(
			this.#host,
			UserService.postUserByIdResetPassword({
				path: { id: unique },
			}),
		);
	}
}
