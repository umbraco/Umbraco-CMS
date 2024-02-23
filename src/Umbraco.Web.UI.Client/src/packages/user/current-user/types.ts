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
	documentStartNodeIds: Array<string>;
	mediaStartNodeIds: Array<string>;
	avatarUrls: Array<string>;
	languages: Array<string>;
	hasAccessToAllLanguages: boolean;
	permissions: Array<DocumentPermissionPresentationModel | UnknownTypePermissionPresentationModel>;
}
