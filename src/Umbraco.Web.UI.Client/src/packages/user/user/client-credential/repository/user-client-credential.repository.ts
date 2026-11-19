import type { UmbUserClientCredentialDataSource } from './data-source/index.js';
import { UmbUserClientCredentialServerDataSource } from './data-source/user-client-credential.server.data-source.js';
import type {
	UmbCreateUserClientCredentialRequestArgs,
	UmbDeleteUserClientCredentialRequestArgs,
	UmbUserClientCredentialModel,
	UmbUserClientCredentialRequestArgs,
} from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbRepositoryBase,
	type UmbDataSourceErrorResponse,
	type UmbRepositoryResponse,
} from '@umbraco-cms/backoffice/repository';

/**
 * UmbUserClientCredentialRepository
 * @class UmbUserClientCredentialRepository
 * @augments {UmbRepositoryBase}
 */
export class UmbUserClientCredentialRepository extends UmbRepositoryBase {
	#source: UmbUserClientCredentialDataSource;

	/**
	 * Creates an instance of UmbUserClientCredentialRepository.
	 * @param {UmbControllerHost} host - The controller host
	 * @memberof UmbUserClientCredentialRepository
	 */
	constructor(host: UmbControllerHost) {
		super(host);
		this.#source = new UmbUserClientCredentialServerDataSource(host);
	}

	/**
	 * Creates a new client credentials for a user
	 * @param {UmbCreateUserClientCredentialRequestArgs} args - The user and client to create the credentials for
	 * @returns {Promise<UmbRepositoryResponse<UmbUserClientCredentialModel>>} The created client credential.
	 * @memberof UmbUserClientCredentialRepository
	 */
	async requestCreate(
		args: UmbCreateUserClientCredentialRequestArgs,
	): Promise<UmbRepositoryResponse<UmbUserClientCredentialModel>> {
		return this.#source.create(args);
	}

	/**
	 * Reads the client credentials for a user
	 * @param {UmbUserClientCredentialRequestArgs} args - The user to read the credentials for
	 * @returns {Promise<UmbRepositoryResponse<Array<UmbUserClientCredentialModel>>>} The client credentials for the user.
	 * @memberof UmbUserClientCredentialRepository
	 */
	async requestClientCredentials(
		args: UmbUserClientCredentialRequestArgs,
	): Promise<UmbRepositoryResponse<Array<UmbUserClientCredentialModel>>> {
		return this.#source.read(args);
	}

	/**
	 * Deletes the client credentials for a user
	 * @param {UmbDeleteUserClientCredentialRequestArgs} args - The user and client unique to delete the credentials for
	 * @returns {Promise<UmbDataSourceErrorResponse>} The result of the delete operation.
	 * @memberof UmbUserClientCredentialRepository
	 */
	async requestDelete(args: UmbDeleteUserClientCredentialRequestArgs): Promise<UmbDataSourceErrorResponse> {
		return this.#source.delete(args);
	}
}

export { UmbUserClientCredentialRepository as api };
