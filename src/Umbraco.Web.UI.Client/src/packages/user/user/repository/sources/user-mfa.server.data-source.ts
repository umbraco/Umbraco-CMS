import { UserService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UserTwoFactorProviderModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { UmbDataSourceErrorResponse, UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

/**
 * A data source for User MFA items that fetches data from the server
 * @class UmbMfaServerDataSource
 */
export class UmbUserMfaServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbMfaServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbMfaServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Request the MFA providers for a user
	 * @param {string} unique The unique id of the user
	 * @returns {Promise<UmbDataSourceResponse<Array<UserTwoFactorProviderModel>>>} The MFA providers for the user
	 * @memberof UmbMfaServerDataSource
	 */
	requestMfaProviders(unique: string): Promise<UmbDataSourceResponse<Array<UserTwoFactorProviderModel>>> {
		if (!unique) throw new Error('User id is missing');

		return tryExecute(
			this.#host,
			UserService.getUserById2Fa({
				path: { id: unique },
			}),
		);
	}

	/**
	 * Disables a MFA provider for a user
	 * @param {string} unique The unique id of the user
	 * @param {string} providerName The name of the provider
	 * @returns {Promise<UmbDataSourceErrorResponse>} The result of disabling the MFA provider
	 * @memberof UmbMfaServerDataSource
	 */
	disableMfaProvider(unique: string, providerName: string): Promise<UmbDataSourceErrorResponse> {
		if (!unique) throw new Error('User id is missing');
		if (!providerName) throw new Error('Provider is missing');

		return tryExecute(
			this.#host,
			UserService.deleteUserById2FaByProviderName({
				path: { id: unique, providerName },
			}),
		);
	}
}
