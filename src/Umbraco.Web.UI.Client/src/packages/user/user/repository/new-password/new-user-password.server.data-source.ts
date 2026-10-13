import { UserService } from '@umbraco-cms/backoffice/external/backend-api';
import type { ResetPasswordUserResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

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
	 * @param {string} unique - The unique id of the user
	 * @returns {Promise<UmbDataSourceResponse<ResetPasswordUserResponseModel>>} The new password
	 * @memberof UmbNewUserPasswordServerDataSource
	 */
	async newPassword(unique: string): Promise<UmbDataSourceResponse<ResetPasswordUserResponseModel>> {
		if (!unique) throw new Error('User unique is missing');

		return tryExecute(
			this.#host,
			UserService.postUserByIdResetPassword({
				path: { id: unique },
			}),
		);
	}
}
