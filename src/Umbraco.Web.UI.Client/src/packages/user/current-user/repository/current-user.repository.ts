import { UmbCurrentUserServerDataSource } from './current-user.server.data-source.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';

/**
 * A repository for the current user
 * @export
 * @class UmbCurrentUserRepository
 * @extends {UmbRepositoryBase}
 */
export class UmbCurrentUserRepository extends UmbRepositoryBase {
	#currentUserSource: UmbCurrentUserServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#currentUserSource = new UmbCurrentUserServerDataSource(host);
	}

	/**
	 * Request the current user
	 * @return {*}
	 * @memberof UmbCurrentUserRepository
	 */
	async requestCurrentUser() {
		// TODO: add observable option
		return this.#currentUserSource.getCurrentUser();
	}
}

export default UmbCurrentUserRepository;
