import type { UmbMockUserModel } from '../../mock-data-set.types.js';
import { UserKindModel, UserStateModel } from '@umbraco-cms/backoffice/external/backend-api';
import { ADMIN_USER_GROUP_ID } from './user-group.data.js';

export const data: Array<UmbMockUserModel> = [
	{
		id: 'variant-documents-admin-user-id',
		name: 'Admin User',
		email: 'admin@example.com',
		userName: '',
		isAdmin: true,
		kind: UserKindModel.DEFAULT,
		state: UserStateModel.ACTIVE,
		languageIsoCode: 'en-us',
		avatarUrls: [],
		createDate: '2024-01-15T10:00:00.000Z',
		updateDate: '2024-01-15T10:00:00.000Z',
		lastLoginDate: '2024-01-15T10:00:00.000Z',
		lastLockoutDate: null,
		lastPasswordChangeDate: '2024-01-15T10:00:00.000Z',
		failedLoginAttempts: 0,
		documentStartNodeIds: [],
		mediaStartNodeIds: [],
		hasDocumentRootAccess: true,
		hasMediaRootAccess: true,
		userGroupIds: [{ id: ADMIN_USER_GROUP_ID }],
		flags: [],
	},
];
