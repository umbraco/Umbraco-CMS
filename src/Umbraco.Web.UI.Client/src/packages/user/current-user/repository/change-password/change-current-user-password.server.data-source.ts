import { UserService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A server data source for changing the password of a user
 * @export
 * @class UmbChangeCurrentUserPasswordServerDataSource
 */
export class UmbChangeCurrentUserPasswordServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbChangeCurrentUserPasswordServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbChangeCurrentUserPasswordServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Change the password of a user
	 * @param {string} id
	 * @param {string} newPassword
	 * @returns {*}
	 * @memberof UmbChangeCurrentUserPasswordServerDataSource
	 */
	async changePassword(id: string, newPassword: string, oldPassword: string, isCurrentUser: boolean) {
		if (!id) throw new Error('User Id is missing');

		if(isCurrentUser){
			return tryExecuteAndNotify(
				this.#host,
				UserService.postCurrentUserByIdChangePassword({
					requestBody: {
						newPassword,
						oldPassword
					},
				}),
			);
		}
		else{
			return tryExecuteAndNotify(
				this.#host,
				UserService.postUserByIdChangePassword({
					id,
					requestBody: {
						newPassword
					},
				}),
			);
		}
	}
}
