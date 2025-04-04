import { UserService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute, tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

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
	 * @param unique The unique id of the user
	 * @memberof UmbMfaServerDataSource
	 */
	requestMfaProviders(unique: string) {
		if (!unique) throw new Error('User id is missing');

		return tryExecute(
			this.#host,
			UserService.getUserById2Fa({
				id: unique,
			}),
		);
	}

	/**
	 * Disables a MFA provider for a user
	 * @param unique The unique id of the user
	 * @param providerName The name of the provider
	 * @memberof UmbMfaServerDataSource
	 */
	disableMfaProvider(unique: string, providerName: string) {
		if (!unique) throw new Error('User id is missing');
		if (!providerName) throw new Error('Provider is missing');

		return tryExecute(
			this.#host,
			UserService.deleteUserById2FaByProviderName({
				id: unique,
				providerName,
			}),
		);
	}
}
