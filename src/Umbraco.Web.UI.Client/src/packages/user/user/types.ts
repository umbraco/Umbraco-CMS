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
	state: UserStateModel;
	failedLoginAttempts: number;
	createDate: string;
	updateDate: string;
	lastLoginDate: string | null;
	lastLockoutDate: string | null;
	lastPasswordChangeDate: string | null;
}
