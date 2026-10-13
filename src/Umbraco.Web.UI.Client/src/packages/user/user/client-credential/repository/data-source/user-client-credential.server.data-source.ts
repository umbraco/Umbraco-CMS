import type {
	UmbCreateUserClientCredentialRequestArgs,
	UmbDeleteUserClientCredentialRequestArgs,
	UmbUserClientCredentialModel,
	UmbUserClientCredentialRequestArgs,
} from '../types.js';
import type { UmbUserClientCredentialDataSource } from './types.js';
import { UserService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDataSourceErrorResponse, UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

/**
 * Server data source for user client credentials
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
	 * @returns {Promise<UmbDataSourceResponse<UmbUserClientCredentialModel>>} The created client credential.
	 * @memberof UmbUserClientCredentialServerDataSource
	 */
	async create(
		args: UmbCreateUserClientCredentialRequestArgs,
	): Promise<UmbDataSourceResponse<UmbUserClientCredentialModel>> {
		const { error } = await tryExecute(
			this.#host,
			UserService.postUserByIdClientCredentials({
				path: { id: args.user.unique },
				body: {
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
	 * @returns {Promise<UmbDataSourceResponse<Array<UmbUserClientCredentialModel>>>} The client credentials for the user.
	 * @memberof UmbUserClientCredentialServerDataSource
	 */
	async read(
		args: UmbUserClientCredentialRequestArgs,
	): Promise<UmbDataSourceResponse<Array<UmbUserClientCredentialModel>>> {
		const { data, error } = await tryExecute(
			this.#host,
			UserService.getUserByIdClientCredentials({
				path: { id: args.user.unique },
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
	 * @returns {Promise<UmbDataSourceErrorResponse>} The result of the delete operation.
	 * @memberof UmbUserClientCredentialServerDataSource
	 */
	delete(args: UmbDeleteUserClientCredentialRequestArgs): Promise<UmbDataSourceErrorResponse> {
		return tryExecute(
			this.#host,
			UserService.deleteUserByIdClientCredentialsByClientId({
				path: { id: args.user.unique, clientId: args.client.unique },
			}),
		);
	}
}
