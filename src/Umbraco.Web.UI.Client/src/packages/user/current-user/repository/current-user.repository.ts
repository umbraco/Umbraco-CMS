import { UmbCurrentUserServerDataSource } from './current-user.server.data-source.js';
import { UMB_CURRENT_USER_STORE_CONTEXT } from './current-user.store.token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { TemporaryFileStatus, UmbTemporaryFileManager } from '@umbraco-cms/backoffice/temporary-file';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';

/**
 * A repository for the current user
 * @class UmbCurrentUserRepository
 * @augments {UmbRepositoryBase}
 */
export class UmbCurrentUserRepository extends UmbRepositoryBase {
	#currentUserSource = new UmbCurrentUserServerDataSource(this._host);
	#currentUserStore?: typeof UMB_CURRENT_USER_STORE_CONTEXT.TYPE;
	#temporaryFileManager = new UmbTemporaryFileManager(this);
	#abortController = new AbortController();
	#localize = new UmbLocalizationController(this);
	#init: Promise<unknown>;
	protected notificationContext?: typeof UMB_NOTIFICATION_CONTEXT.TYPE;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#init = Promise.all([
			this.consumeContext(UMB_CURRENT_USER_STORE_CONTEXT, (instance) => {
				if (instance) {
					this.#currentUserStore = instance;
				}
			})
				.asPromise({ preventTimeout: true })
				// Ignore the error, we can assume that the flow was stopped (asPromise failed), but it does not mean that the consumption was not successful.
				.catch(() => undefined),

			this.consumeContext(UMB_NOTIFICATION_CONTEXT, (instance) => {
				this.notificationContext = instance;
			})
				.asPromise({ preventTimeout: true })
				.catch(() => undefined),
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
			const notification = { data: { message: this.#localize.term('user_passwordChangedGeneric') } };
			this.notificationContext?.peek('positive', notification);
		} else {
			const notification = { data: { message: error.message ?? this.#localize.term('user_unknownFailure') } };
			this.notificationContext?.peek('danger', notification);
		}

		return { data, error };
	}

	/**
	 * Upload an avatar for the current user
	 * @param {File} file - The image file to use as avatar
	 */
	async uploadAvatar(file: File) {
		await this.#init;

		const temporaryUnique = UmbId.new();
		const { status } = await this.#temporaryFileManager.uploadOne({
			file,
			temporaryUnique,
			abortController: this.#abortController,
		});

		if (status === TemporaryFileStatus.ERROR) {
			return { error: new Error('Avatar upload failed') };
		}

		const { error } = await this.#currentUserSource.uploadCurrentUserAvatar(temporaryUnique);

		if (!error) {
			const localUrl = URL.createObjectURL(file);
			// The server returns 5 different sizes of the avatar, so we mimic that here
			this.#currentUserStore?.update({ avatarUrls: [localUrl, localUrl, localUrl, localUrl, localUrl] });

			const notification = { data: { message: this.#localize.term('user_avatarUploadSuccess') } };
			this.notificationContext?.peek('positive', notification);
		}

		return { error };
	}

	/**
	 * Delete the current user's avatar
	 */
	async deleteAvatar() {
		await this.#init;

		const { error } = await this.#currentUserSource.deleteCurrentUserAvatar();

		if (!error) {
			this.#currentUserStore?.update({ avatarUrls: [] });

			const notification = { data: { message: this.#localize.term('user_avatarDeleteSuccess') } };
			this.notificationContext?.peek('positive', notification);
		}

		return { error };
	}

	/**
	 * Update the current user's profile settings
	 * @param languageIsoCode
	 */
	async updateProfile(languageIsoCode: string) {
		await this.#init;

		const { error } = await this.#currentUserSource.updateCurrentUserProfile(languageIsoCode);

		if (error) {
			return { error };
		}

		await this.requestCurrentUser();

		return {};
	}
}

export default UmbCurrentUserRepository;
