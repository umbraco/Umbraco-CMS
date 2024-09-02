import type { UmbUserClientCredentialsDataSource } from './data-source/index.js';
import { UmbUserClientCredentialsServerDataSource } from './data-source/user-client-credentials.server.data-source.js';
import type {
	UmbUserClientCredentialsRepositoryCreateArgs,
	UmbUserClientCredentialsRepositoryDeleteArgs,
	UmbUserClientCredentialsRepositoryReadArgs,
} from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';

/**
 * UmbUserClientCredentialsRepository
 * @export
 * @class UmbUserClientCredentialsRepository
 * @extends {UmbRepositoryBase}
 */
export class UmbUserClientCredentialsRepository extends UmbRepositoryBase {
	#source: UmbUserClientCredentialsDataSource;

	/**
	 * Creates an instance of UmbUserClientCredentialsRepository.
	 * @param {UmbControllerHost} host - The controller host
	 * @memberof UmbUserClientCredentialsRepository
	 */
	constructor(host: UmbControllerHost) {
		super(host);
		this.#source = new UmbUserClientCredentialsServerDataSource(host);
	}

	/**
	 * Creates a new client credentials for a user
	 * @param {UmbUserClientCredentialsRepositoryCreateArgs} args - The user and client to create the credentials for
	 * @returns {*}
	 * @memberof UmbUserClientCredentialsRepository
	 */
	async create(args: UmbUserClientCredentialsRepositoryCreateArgs) {
		return this.#source.create(args);
	}

	/**
	 * Reads the client credentials for a user
	 * @param {UmbUserClientCredentialsRepositoryReadArgs} args - The user to read the credentials for
	 * @returns {*}
	 * @memberof UmbUserClientCredentialsRepository
	 */
	async read(args: UmbUserClientCredentialsRepositoryReadArgs) {
		return this.#source.read(args);
	}

	/**
	 * Deletes the client credentials for a user
	 * @param {UmbUserClientCredentialsRepositoryDeleteArgs} args - The user and client unique to delete the credentials for
	 * @returns {*}
	 * @memberof UmbUserClientCredentialsRepository
	 */
	async delete(args: UmbUserClientCredentialsRepositoryDeleteArgs) {
		return this.#source.delete(args);
	}
}

export { UmbUserClientCredentialsRepository as api };
