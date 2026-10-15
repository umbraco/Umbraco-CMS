import type { UmbMockUserModel } from '../../mock-data-set.types.js';
import { LANGUAGE_ACCESS_SCENARIOS, userGroupId, userGroupName } from './user-group.data.js';
import { UserKindModel, UserStateModel } from '@umbraco-cms/backoffice/external/backend-api';

/**
 * One user per user group, named after the group. The first user is the one the backoffice starts as.
 * Use the "User" switcher in the header to change to another one.
 */
export const data: Array<UmbMockUserModel> = LANGUAGE_ACCESS_SCENARIOS.flatMap((scenario) =>
	[true, false].map((canEditSharedData) => ({
		id: `language-permissions-user-${scenario.key}-${canEditSharedData ? 'can' : 'cannot'}-edit-shared-data-id`,
		name: userGroupName(scenario, canEditSharedData),
		email: `${scenario.key}.${canEditSharedData ? 'can' : 'cannot'}-edit-shared-data@example.com`,
		userName: '',
		isAdmin: false,
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
		elementStartNodeIds: [],
		mediaStartNodeIds: [],
		hasDocumentRootAccess: true,
		hasElementRootAccess: true,
		hasMediaRootAccess: false,
		userGroupIds: [{ id: userGroupId(scenario, canEditSharedData) }],
		flags: [],
	})),
);
