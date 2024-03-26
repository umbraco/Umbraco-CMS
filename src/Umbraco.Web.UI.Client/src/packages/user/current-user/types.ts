import type {
	DocumentPermissionPresentationModel,
	UnknownTypePermissionPresentationModel,
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
