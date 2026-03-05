import type { UmbMockUserModel } from '../../types/mock-data-set.types.js';
import { UserKindModel, UserStateModel } from '@umbraco-cms/backoffice/external/backend-api';

// Map string state to enum
/**
 *
 * @param state
 */
function mapState(state: string): UserStateModel {
	switch (state) {
		case 'Active':
			return UserStateModel.ACTIVE;
		case 'Disabled':
			return UserStateModel.DISABLED;
		case 'LockedOut':
			return UserStateModel.LOCKED_OUT;
		case 'Invited':
			return UserStateModel.INVITED;
		case 'Inactive':
			return UserStateModel.INACTIVE;
		default:
			return UserStateModel.ACTIVE;
	}
}

const rawData = [
	{
		avatarUrls: [],
		createDate: '2023-02-20 15:06:02',
		documentStartNodeIds: [],
		email: 'kja@umbraco.dk',
		failedLoginAttempts: 0,
		hasDocumentRootAccess: true,
		hasMediaRootAccess: true,
		id: '1e70f841-c261-413b-abb2-2d68cdb96094',
		isAdmin: true,
		kind: 'Default',
		languageIsoCode: 'en-US',
		lastLockoutDate: null,
		lastLoginDate: '2024-03-18 13:39:30',
		lastPasswordChangeDate: '2023-02-20 15:06:03',
		mediaStartNodeIds: [],
		name: 'Kenn Jacobsen',
		state: 'Active',
		updateDate: '2024-03-18 13:39:30',
		userGroupIds: [
			{
				id: 'e5e7f6c8-7f9c-4b5b-8d5d-9e1e5a4f7e4d',
			},
			{
				id: '8c6ad70f-d307-4e4a-af58-72c2e4e9439d',
			},
		],
		userName: 'kja@umbraco.dk',
		flags: [],
	},
	{
		avatarUrls: [],
		createDate: '2024-03-18 09:24:08',
		documentStartNodeIds: [],
		email: 'leekelleher@gmail.com',
		failedLoginAttempts: 0,
		hasDocumentRootAccess: true,
		hasMediaRootAccess: true,
		id: '1973816d-5b40-4179-b571-b07204c9b442',
		isAdmin: true,
		kind: 'Default',
		languageIsoCode: 'en-us',
		lastLockoutDate: null,
		lastLoginDate: '2026-01-28 08:49:28.9819432',
		lastPasswordChangeDate: '2024-04-02 13:04:44',
		mediaStartNodeIds: [],
		name: 'Lee Kelleher',
		state: 'Active',
		updateDate: '2026-01-28 08:49:29.1274787',
		userGroupIds: [
			{
				id: 'e5e7f6c8-7f9c-4b5b-8d5d-9e1e5a4f7e4d',
			},
			{
				id: '8c6ad70f-d307-4e4a-af58-72c2e4e9439d',
			},
		],
		userName: 'leekelleher@gmail.com',
		flags: [],
	},
	{
		avatarUrls: [],
		createDate: '2024-03-18 13:53:48',
		documentStartNodeIds: [],
		email: 'lke@umbraco.dk',
		failedLoginAttempts: 0,
		hasDocumentRootAccess: true,
		hasMediaRootAccess: true,
		id: '7f033c76-4026-48df-9a60-55c429890876',
		isAdmin: false,
		kind: 'Default',
		languageIsoCode: 'en-us',
		lastLockoutDate: null,
		lastLoginDate: '2025-11-24 17:59:29.2328312',
		lastPasswordChangeDate: '2024-11-06 16:57:05',
		mediaStartNodeIds: [],
		name: 'Lee Kelleher HQ',
		state: 'Active',
		updateDate: '2025-11-24 17:59:29.4247268',
		userGroupIds: [
			{
				id: '2c75770e-5009-4e8c-b9e0-fb90520862a0',
			},
		],
		userName: 'lke@umbraco.dk',
		flags: [],
	},
	{
		avatarUrls: [],
		createDate: '2025-09-10 09:57:57',
		documentStartNodeIds: [],
		email: 'lke+api@umbraco.dk',
		failedLoginAttempts: 0,
		hasDocumentRootAccess: true,
		hasMediaRootAccess: true,
		id: '80aad090-07bf-41bd-aadb-1d3284886ec1',
		isAdmin: true,
		kind: 'Default',
		languageIsoCode: 'en-us',
		lastLockoutDate: null,
		lastLoginDate: '2025-09-10 10:41:51',
		lastPasswordChangeDate: '2025-09-10 09:57:57',
		mediaStartNodeIds: [],
		name: 'API_Admin',
		state: 'Active',
		updateDate: '2025-09-10 10:41:51',
		userGroupIds: [
			{
				id: 'e5e7f6c8-7f9c-4b5b-8d5d-9e1e5a4f7e4d',
			},
		],
		userName: 'lke+api@umbraco.dk',
		flags: [],
	},
];

export const data: Array<UmbMockUserModel> = rawData.map((user) => ({
	...user,
	kind: UserKindModel.DEFAULT,
	state: mapState(user.state),
}));
