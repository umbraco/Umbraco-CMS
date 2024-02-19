import { UmbCurrentUserServerDataSource } from './current-user.server.data-source.js';
import { UMB_CURRENT_USER_STORE_CONTEXT } from './current-user.store.js';
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
	#currentUserStore?: typeof UMB_CURRENT_USER_STORE_CONTEXT.TYPE;
	#init: Promise<unknown>;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#currentUserSource = new UmbCurrentUserServerDataSource(host);

		this.#init = Promise.all([
			this.consumeContext(UMB_CURRENT_USER_STORE_CONTEXT, (instance) => {
				this.#currentUserStore = instance;
			}).asPromise(),
		]);
	}

	/**
	 * Request the current user
	 * @return {*}
	 * @memberof UmbCurrentUserRepository
	 */
	async requestCurrentUser() {
		await this.#init;
		const { data, error } = await this.#currentUserSource.getCurrentUser();

		if (data) {
			this.#currentUserStore?.set(data);
		}

		return { data, error, asObservable: () => this.#currentUserStore!.data };
	}
}

export default UmbCurrentUserRepository;
