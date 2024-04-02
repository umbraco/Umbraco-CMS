import type { UmbCurrentUserMfaProviderModel } from '../types.js';
import { UmbCurrentUserServerDataSource } from './current-user.server.data-source.js';
import { UMB_CURRENT_USER_STORE_CONTEXT } from './current-user.store.js';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import { UserResource } from '@umbraco-cms/backoffice/external/backend-api';

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

	/**
	 * Request the current user's available MFA login providers
	 * @memberof UmbCurrentUserRepository
	 */
	async requestMfaLoginProviders() {
		await this.#init;

		const { data, error } = await this.#currentUserSource.getMfaLoginProviders();

		if (data) {
			this.#currentUserStore?.setMfaProviders(data);
		}

		return { data, error, asObservable: () => this.#currentUserStore!.mfaProviders };
	}

	/**
	 * Enable an MFA provider
	 * @param provider The provider to enable
	 * @param code The activation code of the provider to enable
	 * @memberof UmbCurrentUserRepository
	 */
	async enableMfaProvider(providerName: string, code: string, secret: string): Promise<boolean> {
		const { error } = await tryExecuteAndNotify(
			this._host,
			UserResource.postUserCurrent2FaByProviderName({ providerName, requestBody: { code, secret } }),
		);

		if (error) {
			return false;
		}

		this.#currentUserStore?.updateMfaProvider({ providerName, isEnabledOnUser: true });

		return true;
	}

	/**
	 * Disable an MFA provider
	 * @param provider The provider to disable
	 * @param code The activation code of the provider to disable
	 * @memberof UmbCurrentUserRepository
	 */
	async disableMfaProvider(providerName: string, code: string): Promise<boolean> {
		const { error } = await tryExecuteAndNotify(
			this._host,
			UserResource.deleteUserCurrent2FaByProviderName({ providerName, code }),
		);

		if (error) {
			return false;
		}

		this.#currentUserStore?.updateMfaProvider({ providerName, isEnabledOnUser: false });

		return true;
	}
}

export default UmbCurrentUserRepository;
