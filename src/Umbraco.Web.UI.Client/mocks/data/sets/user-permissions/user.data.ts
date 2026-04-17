import type { UmbMockUserModel } from '../../mock-data-set.types.js';
import { UserKindModel, UserStateModel } from '@umbraco-cms/backoffice/external/backend-api';

export const data: Array<UmbMockUserModel> = [
	{
		avatarUrls: [],
		createDate: '3/13/2022',
		documentStartNodeIds: [],
		email: 'noreply@umbraco.com',
		failedLoginAttempts: 946,
		hasDocumentRootAccess: true,
		hasMediaRootAccess: true,
		id: 'bca6c733-a63d-4353-a271-9a8b6bcca8bd',
		isAdmin: true,
		kind: UserKindModel.DEFAULT,
		languageIsoCode: 'en-us',
		lastLockoutDate: '11/23/2021',
		lastLoginDate: '9/10/2022',
		lastPasswordChangeDate: '1/10/2022',
		mediaStartNodeIds: [],
		name: 'Umbraco User',
		state: UserStateModel.ACTIVE,
		updateDate: '2/10/2022',
		userGroupIds: [{ id: 'user-group-administrators-id' }],
		userName: '',
		flags: [],
	},
];
