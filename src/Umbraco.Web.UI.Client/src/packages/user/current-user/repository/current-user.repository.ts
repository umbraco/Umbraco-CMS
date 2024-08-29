import { UmbCurrentUserServerDataSource } from './current-user.server.data-source.js';
import { UMB_CURRENT_USER_STORE_CONTEXT } from './current-user.store.token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';

/**
 * A repository for the current user
 * @class UmbCurrentUserRepository
 * @augments {UmbRepositoryBase}
 */
export class UmbCurrentUserRepository extends UmbRepositoryBase {
	#currentUserSource = new UmbCurrentUserServerDataSource(this._host);
	#currentUserStore?: typeof UMB_CURRENT_USER_STORE_CONTEXT.TYPE;
	#init: Promise<unknown>;
	protected notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#init = Promise.all([
			this.consumeContext(UMB_CURRENT_USER_STORE_CONTEXT, (instance) => {
				this.#currentUserStore = instance;
			}).asPromise(),

			this.consumeContext(UMB_NOTIFICATION_CONTEXT, (instance) => {
				this.notificationContext = instance;
			}).asPromise(),
		]);
	}

	/**
	 * Request the current user
	 * @returns {*}
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
	 * Request the current user's external login providers
	 * @memberof UmbCurrentUserRepository
	 */
	async requestExternalLoginProviders() {
		await this.#init;
		const { data, error } = await this.#currentUserSource.getExternalLoginProviders();

		if (data) {
			this.#currentUserStore?.setExternalLoginProviders(data);
		}

		return { data, error, asObservable: () => this.#currentUserStore!.externalLoginProviders };
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
	 * @param providerName
	 * @param code The activation code of the provider to enable
	 * @param secret
	 * @memberof UmbCurrentUserRepository
	 */
	async enableMfaProvider(providerName: string, code: string, secret: string) {
		const { error } = await this.#currentUserSource.enableMfaProvider(providerName, code, secret);

		if (error) {
			return { error };
		}

		this.#currentUserStore?.updateMfaProvider({ providerName, isEnabledOnUser: true });

		return {};
	}

	/**
	 * Disable an MFA provider
	 * @param provider The provider to disable
	 * @param providerName
	 * @param code The activation code of the provider to disable
	 * @memberof UmbCurrentUserRepository
	 */
	async disableMfaProvider(providerName: string, code: string) {
		const { error } = await this.#currentUserSource.disableMfaProvider(providerName, code);

		if (error) {
			return { error };
		}

		this.#currentUserStore?.updateMfaProvider({ providerName, isEnabledOnUser: false });

		return {};
	}
	/**
	 * Change password for current user
	 * @param userId
	 * @param newPassword
	 * @param oldPassword
	 * @param isCurrentUser
	 * @returns
	 */
	async changePassword(newPassword: string, oldPassword: string) {
		if (!newPassword) throw new Error('New password is missing');
		if (!oldPassword) throw new Error('Old password is missing');

		const { data, error } = await this.#currentUserSource.changePassword(newPassword, oldPassword);

		if (!error) {
			const notification = { data: { message: `Password changed` } };
			this.notificationContext?.peek('positive', notification);
		}

		return { data, error };
	}
}

export default UmbCurrentUserRepository;
