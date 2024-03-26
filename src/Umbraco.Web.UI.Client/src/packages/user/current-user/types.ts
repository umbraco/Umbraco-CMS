import type {
	DocumentPermissionPresentationModel,
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
}

export type UmbCurrentUserMfaProviderModel = UserTwoFactorProviderModel;

export interface UmbMfaProviderConfigurationElementProps {
	providerName: string;
	isEnabled: boolean;
	onSubmit: (value: { code: string; secret?: string }) => void;
	onClose: () => void;
}
