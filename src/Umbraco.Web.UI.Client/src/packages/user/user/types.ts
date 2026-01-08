import type { UmbUserEntityType } from './entity.js';
import type { UmbUserKindType } from './utils/index.js';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import {
	type CurrenUserConfigurationResponseModel,
	type UserConfigurationResponseModel,
	UserStateModel,
	type UserTwoFactorProviderModel,
} from '@umbraco-cms/backoffice/external/backend-api';

export type * from './conditions/types.js';

export type UmbUserStateEnum = UserStateModel;
export const UmbUserStateEnum = UserStateModel;

export interface UmbUserDetailModel extends UmbUserStartNodesModel {
	avatarUrls: Array<string>;
	createDate: string | null;
	email: string;
	entityType: UmbUserEntityType;
	failedLoginAttempts: number;
	isAdmin: boolean;
	languageIsoCode: string | null;
	lastLockoutDate: string | null;
	lastLoginDate: string | null;
	lastPasswordChangeDate: string | null;
	name: string;
	state: UmbUserStateEnum | null;
	unique: string;
	updateDate: string | null;
	userGroupUniques: Array<UmbReferenceByUnique>;
	userName: string;
	kind: UmbUserKindType;
}

export interface UmbUserStartNodesModel {
	documentStartNodeUniques: Array<UmbReferenceByUnique>;
	hasDocumentRootAccess: boolean;
	hasMediaRootAccess: boolean;
	mediaStartNodeUniques: Array<UmbReferenceByUnique>;
}

export type UmbUserMfaProviderModel = UserTwoFactorProviderModel;

export type UmbUserConfigurationModel = UserConfigurationResponseModel;

export type UmbCurrentUserConfigurationModel = CurrenUserConfigurationResponseModel;
