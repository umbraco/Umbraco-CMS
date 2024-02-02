import type { UmbUserEntityType } from './entity.js';
import type { UserStateModel } from '@umbraco-cms/backoffice/backend-api';

export interface UmbUserDetailModel {
	entityType: UmbUserEntityType;
	email: string;
	userName: string;
	name: string;
	userGroupIds: Array<string>;
	unique: string;
	languageIsoCode: string | null;
	contentStartNodeIds: Array<string>;
	mediaStartNodeIds: Array<string>;
	avatarUrls: Array<string>;
	state: UserStateModel | null;
	failedLoginAttempts: number;
	createDate: string | null;
	updateDate: string | null;
	lastLoginDate: string | null;
	lastLockoutDate: string | null;
	lastPasswordChangeDate: string | null;
}
