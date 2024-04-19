import type {
	ApiError,
	CancelError,
	DocumentPermissionPresentationModel,
	LinkedLoginModel,
	UnknownTypePermissionPresentationModel,
	UserTwoFactorProviderModel,
} from '@umbraco-cms/backoffice/external/backend-api';

export interface UmbCurrentUserModel {
	unique: string;
	email: string;
	userName: string;
	name: string;
	languageIsoCode: string;
	documentStartNodeUniques: Array<string>;
	mediaStartNodeUniques: Array<string>;
	avatarUrls: Array<string>;
	languages: Array<string>;
	hasAccessToAllLanguages: boolean;
	allowedSections: Array<string>;
	fallbackPermissions: Array<string>;
	permissions: Array<DocumentPermissionPresentationModel | UnknownTypePermissionPresentationModel>;
	isAdmin: boolean;
}

export type UmbCurrentUserExternalLoginProviderModel = LinkedLoginModel;

export type UmbCurrentUserMfaProviderModel = UserTwoFactorProviderModel;

export type UmbMfaProviderConfigurationCallback = Promise<{ error?: ApiError | CancelError }>;

export interface UmbMfaProviderConfigurationElementProps {
	/**
	 * The name of the provider reflecting the provider name in the backend.
	 */
	providerName: string;

	/**
	 * The display name of the provider. If this is not set, the provider name will be used.
	 */
	displayName: string;

	/**
	 * Call this function to execute the action for the given provider, e.g. to enable or disable it.
	 * @param providerName The name of the provider to enable.
	 * @param code The authentication code from the authentication method.
	 * @param secret The secret from the authentication backend.
	 * @returns A promise that resolves when the action is completed with an error if the action failed.
	 */
	callback: (providerName: string, code: string, secret: string) => UmbMfaProviderConfigurationCallback;

	/**
	 * Call this function to close the modal.
	 */
	close: () => void;
}
