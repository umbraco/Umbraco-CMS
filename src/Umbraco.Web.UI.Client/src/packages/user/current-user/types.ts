import type {
	DocumentPermissionPresentationModel,
	UnknownTypePermissionPresentationModel,
	UserExternalLoginProviderModel,
	UserTwoFactorProviderModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import type { UmbApiError, UmbCancelError } from '@umbraco-cms/backoffice/resources';

export type * from './user-profile-app.extension.js';
export type * from './current-user-action.extension.js';
export type * from './conditions/types.js';

export interface UmbCurrentUserModel {
	allowedSections: Array<string>;
	avatarUrls: Array<string>;
	documentStartNodeUniques: Array<UmbReferenceByUnique>;
	email: string;
	fallbackPermissions: Array<string>;
	hasAccessToAllLanguages: boolean;
	hasAccessToSensitiveData: boolean;
	hasDocumentRootAccess: boolean;
	hasMediaRootAccess: boolean;
	isAdmin: boolean;
	languageIsoCode: string;
	languages: Array<string>;
	mediaStartNodeUniques: Array<UmbReferenceByUnique>;
	name: string;
	permissions: Array<DocumentPermissionPresentationModel | UnknownTypePermissionPresentationModel>;
	unique: string;
	userName: string;
	userGroupUniques: string[];
}

export type UmbCurrentUserExternalLoginProviderModel = UserExternalLoginProviderModel;

export type UmbCurrentUserMfaProviderModel = UserTwoFactorProviderModel;

export type UmbMfaProviderConfigurationCallback = Promise<{ error?: UmbApiError | UmbCancelError }>;

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
