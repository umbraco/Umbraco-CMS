import type { UmbCurrentUserModel } from '../types.js';
import type {
	SetAvatarRequestModel,
	UserExternalLoginProviderModel,
	UserTwoFactorProviderModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { UserService } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbManagementApiDataMapper } from '@umbraco-cms/backoffice/repository';
import type { UmbDataSourceErrorResponse, UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the current user that fetches data from the server
 * @class UmbCurrentUserServerDataSource
 */
export class UmbCurrentUserServerDataSource extends UmbControllerBase {
	#dataMapper = new UmbManagementApiDataMapper(this);

	/**
	 * Get the current user
	 * @returns {Promise<UmbDataSourceResponse<UmbCurrentUserModel>>} The current user data or an error
	 * @memberof UmbCurrentUserServerDataSource
	 */
	async getCurrentUser(): Promise<UmbDataSourceResponse<UmbCurrentUserModel>> {
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
				documentStartNodeUniques: data.documentStartNodeIds.map((node) => ({ unique: node.id })),
				elementStartNodeUniques: data.elementStartNodeIds.map((node) => ({ unique: node.id })),
				email: data.email,
				fallbackPermissions: data.fallbackPermissions,
				hasAccessToAllLanguages: data.hasAccessToAllLanguages,
				hasAccessToSensitiveData: data.hasAccessToSensitiveData,
				hasDocumentRootAccess: data.hasDocumentRootAccess,
				hasMediaRootAccess: data.hasMediaRootAccess,
				hasElementRootAccess: data.hasElementRootAccess,
				isAdmin: data.isAdmin,
				languageIsoCode: data.languageIsoCode || 'en-us', // TODO: make global variable
				languages: data.languages,
				mediaStartNodeUniques: data.mediaStartNodeIds.map((node) => ({ unique: node.id })),
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
	 * @returns {Promise<UmbDataSourceResponse<Array<UserExternalLoginProviderModel>>>} The external login providers data or an error
	 * @memberof UmbCurrentUserServerDataSource
	 */
	async getExternalLoginProviders(): Promise<UmbDataSourceResponse<Array<UserExternalLoginProviderModel>>> {
		return tryExecute(this, UserService.getUserCurrentLoginProviders());
	}

	/**
	 * Get the current user's available MFA login providers
	 * @returns {Promise<UmbDataSourceResponse<Array<UserTwoFactorProviderModel>>>} The MFA login providers data or an error
	 * @memberof UmbCurrentUserServerDataSource
	 */
	async getMfaLoginProviders(): Promise<UmbDataSourceResponse<Array<UserTwoFactorProviderModel>>> {
		const { data, error } = await tryExecute(this, UserService.getUserCurrent2Fa());

		if (data) {
			return { data };
		}

		return { error };
	}

	/**
	 * Enable an MFA provider
	 * @param {string} providerName The name of the provider to enable
	 * @param {string} code The activation code of the provider to enable
	 * @param {string} secret The secret used to verify the provider's activation code
	 * @returns {Promise<UmbDataSourceErrorResponse>} An error if the provider could not be enabled
	 */
	async enableMfaProvider(providerName: string, code: string, secret: string): Promise<UmbDataSourceErrorResponse> {
		const { error } = await tryExecute(
			this,
			UserService.postUserCurrent2FaByProviderName({ path: { providerName }, body: { code, secret } }),
		);

		if (error) {
			return { error };
		}

		return {};
	}

	/**
	 * Disable an MFA provider
	 * @param {string} providerName The name of the provider to disable
	 * @param {string} code The activation code of the provider to disable
	 * @returns {Promise<UmbDataSourceErrorResponse>} An error if the provider could not be disabled
	 */
	async disableMfaProvider(providerName: string, code: string): Promise<UmbDataSourceErrorResponse> {
		const { error } = await tryExecute(
			this,
			UserService.deleteUserCurrent2FaByProviderName({ path: { providerName }, query: { code } }),
		);

		if (error) {
			return { error };
		}

		return {};
	}

	/**
	 * Change the password for current user
	 * @param {string} newPassword The new password
	 * @param {string} oldPassword The old password
	 * @returns {Promise<UmbDataSourceResponse<unknown>>} The result of the change password request
	 */
	async changePassword(newPassword: string, oldPassword: string): Promise<UmbDataSourceResponse<unknown>> {
		return tryExecute(
			this,
			UserService.postUserCurrentChangePassword({
				body: {
					newPassword,
					oldPassword,
				},
			}),
			{ disableNotifications: true },
		);
	}

	/**
	 * Upload an avatar for the current user using a temporary file unique
	 * @param {string} fileUnique The unique of the temporary file to use as avatar
	 * @returns {Promise<UmbDataSourceErrorResponse>} An error if the avatar upload failed
	 */
	async uploadCurrentUserAvatar(fileUnique: string): Promise<UmbDataSourceErrorResponse> {
		const body: SetAvatarRequestModel = {
			file: {
				id: fileUnique,
			},
		};

		return tryExecute(this, UserService.postUserCurrentAvatar({ body }));
	}

	/**
	 * Delete the current user's avatar
	 * @returns {Promise<UmbDataSourceErrorResponse>} An error if the avatar deletion failed
	 */
	async deleteCurrentUserAvatar(): Promise<UmbDataSourceErrorResponse> {
		return tryExecute(this, UserService.deleteUserCurrentAvatar());
	}

	/**
	 * Update the current user's profile
	 * @param {string} languageIsoCode The ISO code of the language to set for the current user
	 * @returns {Promise<UmbDataSourceErrorResponse>} An error if the profile update failed
	 */
	async updateCurrentUserProfile(languageIsoCode: string): Promise<UmbDataSourceErrorResponse> {
		return tryExecute(
			this,
			UserService.putUserCurrentProfile({
				body: {
					languageIsoCode,
				},
			}),
		);
	}
}
