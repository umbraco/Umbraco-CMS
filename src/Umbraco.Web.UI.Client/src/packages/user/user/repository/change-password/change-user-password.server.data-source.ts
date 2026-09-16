import { UserService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDataSourceErrorResponse } from '@umbraco-cms/backoffice/repository';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

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
	 * @param {string} id - The unique id of the user.
	 * @param {string} newPassword - The new password to set.
	 * @returns {Promise<UmbDataSourceErrorResponse>} The result of the change password operation.
	 * @memberof UmbChangeUserPasswordServerDataSource
	 */
	async changePassword(id: string, newPassword: string): Promise<UmbDataSourceErrorResponse> {
		if (!id) throw new Error('User Id is missing');

		return tryExecute(
			this.#host,
			UserService.postUserByIdChangePassword({
				path: { id },
				body: {
					newPassword,
				},
			}),
			{ disableNotifications: true },
		);
	}
}
