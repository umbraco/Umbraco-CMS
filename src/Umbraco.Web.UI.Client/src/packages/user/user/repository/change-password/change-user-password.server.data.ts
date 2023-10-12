import { UserResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for Data Type items that fetches data from the server
 * @export
 * @class UmbChangeUserPasswordServerDataSource
 */
export class UmbChangeUserPasswordServerDataSource {
	#host: UmbControllerHostElement;

	/**
	 * Creates an instance of UmbChangeUserPasswordServerDataSource.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbChangeUserPasswordServerDataSource
	 */
	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	/**
	 * Change the password of a user
	 * @param {string} id
	 * @param {string} oldPassword
	 * @param {string} newPassword
	 * @return {*}
	 * @memberof UmbChangeUserPasswordServerDataSource
	 */
	async changePassword(id: string, oldPassword: string, newPassword: string) {
		if (!id) throw new Error('User Id is missing');

		return tryExecuteAndNotify(
			this.#host,
			UserResource.postUserChangePasswordById({
				id,
				requestBody: {
					oldPassword,
					newPassword,
				},
			}),
		);
	}
}
