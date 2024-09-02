import type {
	UmbUserClientCredentialsDataSource,
	UmbUserClientCredentialsDataSourceCreateArgs,
	UmbUserClientCredentialsDataSourceDeleteArgs,
	UmbUserClientCredentialsDataSourceReadArgs,
} from './types.js';
import { UserService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * Server data source for user client credentials
 * @export
 * @class UmbUserClientCredentialsServerDataSource
 * @implements {UmbUserClientCredentialsDataSource}
 */
export class UmbUserClientCredentialsServerDataSource implements UmbUserClientCredentialsDataSource {
	#host: UmbControllerHost;

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Creates a new client credentials for a user
	 * @param {UmbUserClientCredentialsDataSourceCreateArgs} args - The user and client to create the credentials for
	 * @returns {*}
	 * @memberof UmbUserClientCredentialsServerDataSource
	 */
	create(args: UmbUserClientCredentialsDataSourceCreateArgs) {
		return tryExecuteAndNotify(
			this.#host,
			UserService.postUserByIdClientCredentials({
				id: args.user.unique,
				requestBody: {
					clientId: args.client.unique,
					clientSecret: args.client.secret,
				},
			}),
		);
	}

	/**
	 * Reads the client credentials for a user
	 * @param {UmbUserClientCredentialsDataSourceReadArgs} args - The user to read the credentials for
	 * @returns {*}
	 * @memberof UmbUserClientCredentialsServerDataSource
	 */
	read(args: UmbUserClientCredentialsDataSourceReadArgs) {
		return tryExecuteAndNotify(
			this.#host,
			UserService.getUserByIdClientCredentials({
				id: args.user.unique,
			}),
		);
	}

	/**
	 * Deletes the client credentials for a user
	 * @param {UmbUserClientCredentialsDataSourceDeleteArgs} args - The user and client unique to delete the credentials for
	 * @returns {*}
	 * @memberof UmbUserClientCredentialsServerDataSource
	 */
	delete(args: UmbUserClientCredentialsDataSourceDeleteArgs) {
		return tryExecuteAndNotify(
			this.#host,
			UserService.deleteUserByIdClientCredentialsByClientId({
				id: args.user.unique,
				clientId: args.client.unique,
			}),
		);
	}
}
