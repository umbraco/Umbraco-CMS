import type { UmbDirectionModel } from '@umbraco-cms/backoffice/models';

export interface UmbUserCollectionFilterModel {
	skip?: number;
	take?: number;
	orderBy?: UmbUserOrderByModel;
	orderDirection?: UmbDirectionModel;
	userGroupIds?: string[];
	userStates?: UmbUserStateFilterModel[];
	filter?: string;
}

export interface UmbUserOrderByOption {
	unique: string;
	label: string;
	config: {
		orderBy: UmbUserOrderByModel;
		orderDirection: UmbDirectionModel;
	};
}

export enum UmbUserOrderByModel {
	NAME = 'Name',
	CREATE_DATE = 'CreateDate',
	LAST_LOGIN_DATE = 'LastLoginDate',
}

export enum UmbUserStateFilterModel {
	ACTIVE = 'Active',
	DISABLED = 'Disabled',
	LOCKED_OUT = 'LockedOut',
	INVITED = 'Invited',
	INACTIVE = 'Inactive',
	ALL = 'All',
}
