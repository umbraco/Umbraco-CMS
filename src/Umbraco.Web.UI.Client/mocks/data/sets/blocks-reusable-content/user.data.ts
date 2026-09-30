import type { UmbMockUserModel } from '../../mock-data-set.types.js';
import { ADMIN_USER_GROUP_ID, DANISH_EDITOR_USER_GROUP_ID } from './user-group.data.js';
import { UserKindModel, UserStateModel } from '@umbraco-cms/backoffice/external/backend-api';

const adminUser: UmbMockUserModel = {
	id: 'blocks-reusable-content-admin-user-id',
	name: 'Admin User',
	email: 'admin@example.com',
	userName: '',
	isAdmin: true,
	kind: UserKindModel.DEFAULT,
	state: UserStateModel.ACTIVE,
	languageIsoCode: 'en-US',
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
	hasMediaRootAccess: true,
	userGroupIds: [{ id: ADMIN_USER_GROUP_ID }],
	flags: [],
};

const danishEditorUser: UmbMockUserModel = {
	id: 'blocks-reusable-content-danish-editor-user-id',
	name: 'Danish Editor',
	email: 'danish.editor@example.com',
	userName: '',
	isAdmin: false,
	kind: UserKindModel.DEFAULT,
	state: UserStateModel.ACTIVE,
	languageIsoCode: 'en-US',
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
	hasMediaRootAccess: true,
	userGroupIds: [{ id: DANISH_EDITOR_USER_GROUP_ID }],
	flags: [],
};

export const data: Array<UmbMockUserModel> = [adminUser, danishEditorUser];
