import type { UmbCurrentUserModel } from '../types.js';
import { UserService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbManagementApiDataMapper } from '@umbraco-cms/backoffice/repository';

/**
 * A data source for the current user that fetches data from the server
 * @class UmbCurrentUserServerDataSource
 */
export class UmbCurrentUserServerDataSource extends UmbControllerBase {
	#dataMapper = new UmbManagementApiDataMapper(this);

	/**
	 * Get the current user
	 * @returns {*}
	 * @memberof UmbCurrentUserServerDataSource
	 */
	async getCurrentUser() {
		const { data, error } = await tryExecute(this, UserService.getUserCurrent());

		if (data) {
			const permissionDataPromises = data.permissions.map(async (item) => {
				return this.#dataMapper.map({
					forDataModel: item.$type,
					data: item,
					fallback: async () => {
						return {
							...item,
							permissionType: 'unknown',
						};
					},
				});
			});

			const permissions = await Promise.all(permissionDataPromises);

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
				permissions,
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
		return tryExecute(this, UserService.getUserCurrentLoginProviders());
	}

	/**
	 * Get the current user's available MFA login providers
	 * @memberof UmbCurrentUserServerDataSource
	 */
	async getMfaLoginProviders() {
		const { data, error } = await tryExecute(this, UserService.getUserCurrent2Fa());

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
			this,
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
		const { error } = await tryExecute(this, UserService.deleteUserCurrent2FaByProviderName({ providerName, code }));

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
			this,
			UserService.postUserCurrentChangePassword({
				requestBody: {
					newPassword,
					oldPassword,
				},
			}),
		);
	}
}
