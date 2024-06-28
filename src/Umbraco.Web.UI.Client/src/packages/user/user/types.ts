import type { UmbUserEntityType } from './entity.js';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import { UserStateModel, type UserTwoFactorProviderModel } from '@umbraco-cms/backoffice/external/backend-api';

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
}

export interface UmbUserStartNodesModel {
	documentStartNodeUniques: Array<UmbReferenceByUnique>;
	hasDocumentRootAccess: boolean;
	hasMediaRootAccess: boolean;
	mediaStartNodeUniques: Array<UmbReferenceByUnique>;
}

export type UmbUserMfaProviderModel = UserTwoFactorProviderModel;
