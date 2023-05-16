import type { UmbEntityBase } from '@umbraco-cms/backoffice/models';

export interface UserEntity extends UmbEntityBase {
	type: 'user';
}

export type UserStatus = 'enabled' | 'inactive' | 'invited' | 'disabled';

export interface UmbLoggedInUser extends UserEntity {
	email: string;
	status: UserStatus;
	language: string;
	lastLoginDate?: string;
	lastLockoutDate?: string;
	lastPasswordChangeDate?: string;
	updateDate: string;
	createDate: string;
	failedLoginAttempts: number;
	userGroups: Array<string>;
	contentStartNodes: Array<string>;
	mediaStartNodes: Array<string>;
}
