import type { UmbMockUserModel } from '../../mock-data-set.types.js';
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
		email: 'admin@example.com',
		failedLoginAttempts: 0,
		hasDocumentRootAccess: true,
		hasMediaRootAccess: true,
		id: '1e70f841-c261-413b-abb2-2d68cdb96094',
		isAdmin: true,
		kind: 'Default',
		languageIsoCode: 'en-US',
		lastLockoutDate: null,
		lastLoginDate: '2026-04-16 12:20:32.9618828',
		lastPasswordChangeDate: '2026-04-16 09:59:24.5757688',
		mediaStartNodeIds: [],
		name: 'Administrator',
		state: 'Active',
		updateDate: '2026-04-16 12:20:33.1668396',
		userGroupIds: [
			{
				id: 'e5e7f6c8-7f9c-4b5b-8d5d-9e1e5a4f7e4d',
			},
			{
				id: '8c6ad70f-d307-4e4a-af58-72c2e4e9439d',
			},
		],
		userName: 'admin@example.com',
		flags: [],
	},
];

export const data: Array<UmbMockUserModel> = rawData.map((user) => ({
	...user,
	kind: UserKindModel.DEFAULT,
	state: mapState(user.state),
}));
