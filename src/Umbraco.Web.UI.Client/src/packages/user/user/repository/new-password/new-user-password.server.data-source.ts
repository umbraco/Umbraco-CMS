import { UserResource } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A server data source for generating and assigning a new password for a user
 * @export
 * @class UmbNewUserPasswordServerDataSource
 */
export class UmbNewUserPasswordServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbNewUserPasswordServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbNewUserPasswordServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Generate a new password for a user
	 * @param {string} unique
	 * @return {*}
	 * @memberof UmbNewUserPasswordServerDataSource
	 */
	async newPassword(unique: string) {
		if (!unique) throw new Error('User unique is missing');

		return tryExecuteAndNotify(
			this.#host,
			UserResource.postUserByIdResetPassword({
				id: unique,
			}),
		);
	}
}
