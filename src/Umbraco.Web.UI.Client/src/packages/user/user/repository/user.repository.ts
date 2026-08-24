import { UmbUserMfaServerDataSource } from './sources/user-mfa.server.data-source.js';
import { UmbUserSetGroupsServerDataSource } from './sources/user-set-group.server.data-source.js';
import { UmbUserRepositoryBase } from './user-repository-base.js';
import { of } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import type { UserTwoFactorProviderModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbRepositoryResponse, UmbRepositoryResponseWithAsObservable } from '@umbraco-cms/backoffice/repository';

export class UmbUserRepository extends UmbUserRepositoryBase {
	#setUserGroupsSource = new UmbUserSetGroupsServerDataSource(this._host);
	#userMfaSource = new UmbUserMfaServerDataSource(this._host);

	async setUserGroups(userIds: Array<string>, userGroupIds: Array<string>) {
		if (userGroupIds.length === 0) throw new Error('User group ids are missing');
		if (userIds.length === 0) throw new Error('User ids are missing');

		const { error } = await this.#setUserGroupsSource.setGroups(userIds, userGroupIds);

		if (!error) {
			//TODO: Update relevant stores
		}

		return { error };
	}

	/**
	 * Request the MFA providers for a user
	 * @param {string} unique The unique id of the user
	 * @returns {Promise<UmbRepositoryResponseWithAsObservable<Array<UserTwoFactorProviderModel> | undefined, Array<UserTwoFactorProviderModel>>>} The MFA providers for the user
	 * @memberof UmbUserRepository
	 */
	async requestMfaProviders(
		unique: string,
	): Promise<
		UmbRepositoryResponseWithAsObservable<
			Array<UserTwoFactorProviderModel> | undefined,
			Array<UserTwoFactorProviderModel>
		>
	> {
		const { data, error } = await this.#userMfaSource.requestMfaProviders(unique);
		return { data, error, asObservable: () => of(data ?? []) };
	}

	/**
	 * Disables a MFA provider for a user
	 * @param {string} unique The unique id of the user
	 * @param {string} providerName The name of the provider
	 * @param {string} displayName The display name of the provider to show in the notification (optional)
	 * @returns {Promise<UmbRepositoryResponse<unknown>>} The result of disabling the MFA provider
	 * @memberof UmbUserRepository
	 */
	async disableMfaProvider(
		unique: string,
		providerName: string,
		displayName?: string,
	): Promise<UmbRepositoryResponse<unknown>> {
		const { error } = await this.#userMfaSource.disableMfaProvider(unique, providerName);

		const localize = new UmbLocalizationController(this._host);

		if (!error) {
			const notification = {
				data: { message: localize.term('user_2faProviderIsDisabledMsg', displayName ?? providerName) },
			};
			this.notificationContext?.peek('positive', notification);
		} else {
			console.error('Failed to disable MFA provider', error);
			const notification = {
				data: { message: localize.term('user_2faProviderIsNotDisabledMsg', displayName ?? providerName) },
			};
			this.notificationContext?.peek('warning', notification);
		}

		return { error };
	}
}
