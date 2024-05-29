import type { UmbCurrentUserModel } from '../types.js';
import { UserService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute, tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the current user that fetches data from the server
 * @export
 * @class UmbCurrentUserServerDataSource
 */
export class UmbCurrentUserServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbCurrentUserServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbCurrentUserServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Get the current user
	 * @return {*}
	 * @memberof UmbCurrentUserServerDataSource
	 */
	async getCurrentUser() {
		const { data, error } = await tryExecuteAndNotify(this.#host, UserService.getUserCurrent());

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
		return tryExecuteAndNotify(this.#host, UserService.getUserCurrentLoginProviders());
	}

	/**
	 * Get the current user's available MFA login providers
	 * @memberof UmbCurrentUserServerDataSource
	 */
	async getMfaLoginProviders() {
		const { data, error } = await tryExecuteAndNotify(this.#host, UserService.getUserCurrent2Fa());

		if (data) {
			return { data };
		}

		return { error };
	}

	/**
	 * Enable an MFA provider
	 */
	async enableMfaProvider(providerName: string, code: string, secret: string) {
		const { error } = await tryExecute(
			UserService.postUserCurrent2FaByProviderName({ providerName, requestBody: { code, secret } }),
		);

		if (error) {
			return { error };
		}

		return {};
	}

	/**
	 * Disable an MFA provider
	 */
	async disableMfaProvider(providerName: string, code: string) {
		const { error } = await tryExecute(UserService.deleteUserCurrent2FaByProviderName({ providerName, code }));

		if (error) {
			return { error };
		}

		return {};
	}
}
