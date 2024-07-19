export type UmbUserOrderByType = 'Name' | 'CreateDate' | 'LastLoginDate';

export const UmbUserOrderBy = Object.freeze({
	NAME: 'Name',
	CREATE_DATE: 'CreateDate',
	LAST_LOGIN_DATE: 'LastLoginDate',
});

export type UmbUserStateFilterType = 'Active' | 'Disabled' | 'LockedOut' | 'Invited' | 'Inactive' | 'All';

export const UmbUserStateFilter = Object.freeze({
	ACTIVE: 'Active',
	DISABLED: 'Disabled',
	LOCKED_OUT: 'LockedOut',
	INVITED: 'Invited',
	INACTIVE: 'Inactive',
});
