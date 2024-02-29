import type { UmbUserEntityType } from './entity.js';
import { UserStateModel } from '@umbraco-cms/backoffice/external/backend-api';

export type UmbUserStateEnum = UserStateModel;
export const UmbUserStateEnum = UserStateModel;

export interface UmbUserDetailModel {
	entityType: UmbUserEntityType;
	email: string;
	userName: string;
	name: string;
	userGroupUniques: Array<string>;
	unique: string;
	languageIsoCode: string | null;
	documentStartNodeUniques: Array<string>;
	mediaStartNodeUniques: Array<string>;
	avatarUrls: Array<string>;
	state: UmbUserStateEnum | null;
	failedLoginAttempts: number;
	createDate: string | null;
	updateDate: string | null;
	lastLoginDate: string | null;
	lastLockoutDate: string | null;
	lastPasswordChangeDate: string | null;
}
