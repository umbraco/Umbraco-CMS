import { UserService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A server data source for changing the password of a user
 * @class UmbChangeUserPasswordServerDataSource
 */
export class UmbChangeUserPasswordServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbChangeUserPasswordServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbChangeUserPasswordServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Change the password of a user
	 * @param {string} id
	 * @param {string} newPassword
	 * @returns {*}
	 * @memberof UmbChangeUserPasswordServerDataSource
	 */
	async changePassword(id: string, newPassword: string) {
		if (!id) throw new Error('User Id is missing');

		return tryExecuteAndNotify(
			this.#host,
			UserService.postUserByIdChangePassword({
				id,
				requestBody: {
					newPassword,
				},
			}),
		);
	}
}
