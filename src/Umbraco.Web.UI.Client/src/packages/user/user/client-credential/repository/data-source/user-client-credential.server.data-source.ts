import type {
	UmbCreateUserClientCredentialRequestArgs,
	UmbDeleteUserClientCredentialRequestArgs,
	UmbUserClientCredentialRequestArgs,
} from '../types.js';
import type { UmbUserClientCredentialDataSource } from './types.js';
import { UserService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * Server data source for user client credentials
 * @export
 * @class UmbUserClientCredentialServerDataSource
 * @implements {UmbUserClientCredentialDataSource}
 */
export class UmbUserClientCredentialServerDataSource implements UmbUserClientCredentialDataSource {
	#host: UmbControllerHost;

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Creates a new client credentials for a user
	 * @param {UmbCreateUserClientCredentialRequestArgs} args - The user and client to create the credentials for
	 * @returns {*}
	 * @memberof UmbUserClientCredentialServerDataSource
	 */
	async create(args: UmbCreateUserClientCredentialRequestArgs) {
		const { error } = await tryExecuteAndNotify(
			this.#host,
			UserService.postUserByIdClientCredentials({
				id: args.user.unique,
				requestBody: {
					clientId: args.client.unique,
					clientSecret: args.client.secret,
				},
			}),
		);

		if (!error) {
			return { data: { unique: args.client.unique } };
		}

		return { error };
	}

	/**
	 * Reads the client credentials for a user
	 * @param {UmbUserClientCredentialRequestArgs} args - The user to read the credentials for
	 * @returns {*}
	 * @memberof UmbUserClientCredentialServerDataSource
	 */
	async read(args: UmbUserClientCredentialRequestArgs) {
		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			UserService.getUserByIdClientCredentials({
				id: args.user.unique,
			}),
		);

		if (data) {
			const credentials = data.map((clientId) => ({
				unique: clientId,
			}));

			return { data: credentials };
		}

		return { error };
	}

	/**
	 * Deletes the client credentials for a user
	 * @param {UmbDeleteUserClientCredentialRequestArgs} args - The user and client unique to delete the credentials for
	 * @returns {*}
	 * @memberof UmbUserClientCredentialServerDataSource
	 */
	delete(args: UmbDeleteUserClientCredentialRequestArgs) {
		return tryExecuteAndNotify(
			this.#host,
			UserService.deleteUserByIdClientCredentialsByClientId({
				id: args.user.unique,
				clientId: args.client.unique,
			}),
		);
	}
}
