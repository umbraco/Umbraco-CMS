import type {
	UmbCurrentUserExternalLoginProviderModel,
	UmbCurrentUserMfaProviderModel,
	UmbCurrentUserModel,
} from '../types.js';
import { UmbCurrentUserServerDataSource } from './current-user.server.data-source.js';
import { UMB_CURRENT_USER_STORE_CONTEXT } from './current-user.store.token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type {
	UmbDataSourceErrorResponse,
	UmbRepositoryResponse,
	UmbRepositoryResponseWithAsObservable,
} from '@umbraco-cms/backoffice/repository';
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
	 * @returns {Promise<UmbRepositoryResponseWithAsObservable<UmbCurrentUserModel | undefined>>} The current user data, error and an observable of the current user store
	 * @memberof UmbCurrentUserRepository
	 */
	async requestCurrentUser(): Promise<UmbRepositoryResponseWithAsObservable<UmbCurrentUserModel | undefined>> {
		await this.#init;
		const { data, error } = await this.#currentUserSource.getCurrentUser();

		if (data) {
			this.#currentUserStore?.set(data);
		}

		return { data, error, asObservable: () => this.#currentUserStore!.data };
	}

	/**
	 * Request the current user's external login providers
	 * @returns {Promise<UmbRepositoryResponseWithAsObservable<Array<UmbCurrentUserExternalLoginProviderModel> | undefined, Array<UmbCurrentUserExternalLoginProviderModel>>>} The external login providers data, error and an observable of the external login providers store
	 * @memberof UmbCurrentUserRepository
	 */
	async requestExternalLoginProviders(): Promise<
		UmbRepositoryResponseWithAsObservable<
			Array<UmbCurrentUserExternalLoginProviderModel> | undefined,
			Array<UmbCurrentUserExternalLoginProviderModel>
		>
	> {
		await this.#init;
		const { data, error } = await this.#currentUserSource.getExternalLoginProviders();

		if (data) {
			this.#currentUserStore?.setExternalLoginProviders(data);
		}

		return { data, error, asObservable: () => this.#currentUserStore!.externalLoginProviders };
	}

	/**
	 * Request the current user's available MFA login providers
	 * @returns {Promise<UmbRepositoryResponseWithAsObservable<Array<UmbCurrentUserMfaProviderModel> | undefined, Array<UmbCurrentUserMfaProviderModel>>>} The MFA login providers data, error and an observable of the MFA providers store
	 * @memberof UmbCurrentUserRepository
	 */
	async requestMfaLoginProviders(): Promise<
		UmbRepositoryResponseWithAsObservable<
			Array<UmbCurrentUserMfaProviderModel> | undefined,
			Array<UmbCurrentUserMfaProviderModel>
		>
	> {
		await this.#init;

		const { data, error } = await this.#currentUserSource.getMfaLoginProviders();

		if (data) {
			this.#currentUserStore?.setMfaProviders(data);
		}

		return { data, error, asObservable: () => this.#currentUserStore!.mfaProviders };
	}

	/**
	 * Enable an MFA provider
	 * @param {string} providerName The name of the provider to enable
	 * @param {string} code The activation code of the provider to enable
	 * @param {string} secret The secret used to verify the provider's activation code
	 * @returns {Promise<UmbDataSourceErrorResponse>} An error if the provider could not be enabled
	 * @memberof UmbCurrentUserRepository
	 */
	async enableMfaProvider(providerName: string, code: string, secret: string): Promise<UmbDataSourceErrorResponse> {
		const { error } = await this.#currentUserSource.enableMfaProvider(providerName, code, secret);

		if (error) {
			return { error };
		}

		this.#currentUserStore?.updateMfaProvider({ providerName, isEnabledOnUser: true });

		return {};
	}

	/**
	 * Disable an MFA provider
	 * @param {string} providerName The name of the provider to disable
	 * @param {string} code The activation code of the provider to disable
	 * @returns {Promise<UmbDataSourceErrorResponse>} An error if the provider could not be disabled
	 * @memberof UmbCurrentUserRepository
	 */
	async disableMfaProvider(providerName: string, code: string): Promise<UmbDataSourceErrorResponse> {
		const { error } = await this.#currentUserSource.disableMfaProvider(providerName, code);

		if (error) {
			return { error };
		}

		this.#currentUserStore?.updateMfaProvider({ providerName, isEnabledOnUser: false });

		return {};
	}
	/**
	 * Change password for current user
	 * @param {string} newPassword The new password
	 * @param {string} oldPassword The old password
	 * @returns {Promise<UmbRepositoryResponse<unknown>>} The result of the change password request
	 */
	async changePassword(newPassword: string, oldPassword: string): Promise<UmbRepositoryResponse<unknown>> {
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
	 * @returns {Promise<UmbDataSourceErrorResponse>} An error if the avatar upload failed
	 */
	async uploadAvatar(file: File): Promise<UmbDataSourceErrorResponse> {
		await this.#init;

		const temporaryUnique = UmbId.new();
		const { status } = await this.#temporaryFileManager.uploadOne({
			file,
			temporaryUnique,
			abortController: this.#abortController,
		});

		if (status === TemporaryFileStatus.ERROR) {
			const error = new Error('Avatar upload failed');
			this.#peekError(error);
			return { error };
		}

		const { error } = await this.#currentUserSource.uploadCurrentUserAvatar(temporaryUnique);

		if (error) {
			this.#peekError(error);
			return { error };
		}

		// Refresh from server so the store holds the real resized avatar URLs (not a local blob).
		await this.requestCurrentUser();

		this.notificationContext?.peek('positive', {
			data: { message: this.#localize.term('user_avatarUploadSuccess') },
		});

		return { error: undefined };
	}

	/**
	 * Delete the current user's avatar
	 * @returns {Promise<UmbDataSourceErrorResponse>} An error if the avatar deletion failed
	 */
	async deleteAvatar(): Promise<UmbDataSourceErrorResponse> {
		await this.#init;

		const { error } = await this.#currentUserSource.deleteCurrentUserAvatar();

		if (error) {
			this.#peekError(error);
			return { error };
		}

		this.#currentUserStore?.update({ avatarUrls: [] });

		this.notificationContext?.peek('positive', {
			data: { message: this.#localize.term('user_avatarDeleteSuccess') },
		});

		return { error: undefined };
	}

	/**
	 * Update the current user's profile settings
	 * @param {string} languageIsoCode The ISO code of the language to set for the current user
	 * @returns {Promise<UmbDataSourceErrorResponse>} An error if the profile update failed
	 */
	async updateProfile(languageIsoCode: string): Promise<UmbDataSourceErrorResponse> {
		await this.#init;

		const { error } = await this.#currentUserSource.updateCurrentUserProfile(languageIsoCode);

		if (error) {
			this.#peekError(error);
			return { error };
		}

		await this.requestCurrentUser();

		return { error: undefined };
	}

	#peekError(error: { message?: string } | Error) {
		const message = error.message ?? this.#localize.term('user_unknownFailure');
		this.notificationContext?.peek('danger', { data: { message } });
	}
}

export default UmbCurrentUserRepository;
