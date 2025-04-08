import type { UmbCurrentUserModel } from '../types.js';
import { UserService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the current user that fetches data from the server
 * @class UmbCurrentUserServerDataSource
 */
export class UmbCurrentUserServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbCurrentUserServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbCurrentUserServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Get the current user
	 * @returns {*}
	 * @memberof UmbCurrentUserServerDataSource
	 */
	async getCurrentUser() {
		const { data, error } = await tryExecute(this.#host, UserService.getUserCurrent());

		if (data) {
			const user: UmbCurrentUserModel = {
				allowedSections: data.allowedSections,
				avatarUrls: data.avatarUrls,
				documentStartNodeUniques: data.documentStartNodeIds.map((node) => {
					return {
						unique: node.id,
					};
				}),
				email: data.email,
				fallbackPermissions: data.fallbackPermissions,
				hasAccessToAllLanguages: data.hasAccessToAllLanguages,
				hasAccessToSensitiveData: data.hasAccessToSensitiveData,
				hasDocumentRootAccess: data.hasDocumentRootAccess,
				hasMediaRootAccess: data.hasMediaRootAccess,
				isAdmin: data.isAdmin,
				languageIsoCode: data.languageIsoCode || 'en-us', // TODO: make global variable
				languages: data.languages,
				mediaStartNodeUniques: data.mediaStartNodeIds.map((node) => {
					return {
						unique: node.id,
					};
				}),
				name: data.name,
				permissions: data.permissions,
				unique: data.id,
				userName: data.userName,
				userGroupUniques: data.userGroupIds.map((group) => group.id),
			};
			return { data: user };
		}

		return { error };
	}

	/**
	 * Get the current user's external login providers
	 * @memberof UmbCurrentUserServerDataSource
	 */
	async getExternalLoginProviders() {
		return tryExecute(this.#host, UserService.getUserCurrentLoginProviders());
	}

	/**
	 * Get the current user's available MFA login providers
	 * @memberof UmbCurrentUserServerDataSource
	 */
	async getMfaLoginProviders() {
		const { data, error } = await tryExecute(this.#host, UserService.getUserCurrent2Fa());

		if (data) {
			return { data };
		}

		return { error };
	}

	/**
	 * Enable an MFA provider
	 * @param providerName
	 * @param code
	 * @param secret
	 */
	async enableMfaProvider(providerName: string, code: string, secret: string) {
		const { error } = await tryExecute(
			this.#host,
			UserService.postUserCurrent2FaByProviderName({ providerName, requestBody: { code, secret } }),
		);

		if (error) {
			return { error };
		}

		return {};
	}

	/**
	 * Disable an MFA provider
	 * @param providerName
	 * @param code
	 */
	async disableMfaProvider(providerName: string, code: string) {
		const { error } = await tryExecute(
			this.#host,
			UserService.deleteUserCurrent2FaByProviderName({ providerName, code }),
		);

		if (error) {
			return { error };
		}

		return {};
	}

	/**
	 * Change the password for current user
	 * @param id
	 * @param newPassword
	 * @param oldPassword
	 * @param isCurrentUser
	 * @returns
	 */
	async changePassword(newPassword: string, oldPassword: string) {
		return tryExecute(
			this.#host,
			UserService.postUserCurrentChangePassword({
				requestBody: {
					newPassword,
					oldPassword,
				},
			}),
		);
	}
}
