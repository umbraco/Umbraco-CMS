import type { UmbUserClientCredentialDataSource } from './data-source/index.js';
import { UmbUserClientCredentialServerDataSource } from './data-source/user-client-credential.server.data-source.js';
import type {
	UmbCreateUserClientCredentialRequestArgs,
	UmbDeleteUserClientCredentialRequestArgs,
	UmbUserClientCredentialRequestArgs,
} from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';

/**
 * UmbUserClientCredentialRepository
 * @export
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
	 * @returns {*}
	 * @memberof UmbUserClientCredentialRepository
	 */
	async requestCreate(args: UmbCreateUserClientCredentialRequestArgs) {
		return this.#source.create(args);
	}

	/**
	 * Reads the client credentials for a user
	 * @param {UmbUserClientCredentialRequestArgs} args - The user to read the credentials for
	 * @returns {*}
	 * @memberof UmbUserClientCredentialRepository
	 */
	async requestClientCredentials(args: UmbUserClientCredentialRequestArgs) {
		return this.#source.read(args);
	}

	/**
	 * Deletes the client credentials for a user
	 * @param {UmbDeleteUserClientCredentialRequestArgs} args - The user and client unique to delete the credentials for
	 * @returns {*}
	 * @memberof UmbUserClientCredentialRepository
	 */
	async requestDelete(args: UmbDeleteUserClientCredentialRequestArgs) {
		return this.#source.delete(args);
	}
}

export { UmbUserClientCredentialRepository as api };
