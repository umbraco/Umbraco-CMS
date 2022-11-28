import { Entity } from '../mocks/data/entities';

import type { UserStatus } from '../../backoffice/sections/users/user-extensions';

// Extension Manifests
export * from '../extensions-registry/models';

// eslint-disable-next-line @typescript-eslint/no-explicit-any
export type HTMLElementConstructor<T = HTMLElement> = new (...args: any[]) => T;

// Users
export interface UserEntity extends Entity {
	type: 'user';
}

export interface UserDetails extends UserEntity {
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

export interface UserGroupEntity extends Entity {
	type: 'userGroup';
}

export interface UserGroupDetails extends UserGroupEntity {
	sections: Array<string>;
	contentStartNode?: string;
	mediaStartNode?: string;
	permissions: Array<string>;
}
