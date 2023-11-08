import { UmbUserRepository } from './user.repository.js';
import { UmbUserItemStore } from './item/user-item.store.js';
import { UmbUserStore } from './user.store.js';
import { UmbDisableUserRepository } from './disable/disable-user.repository.js';
import { UmbEnableUserRepository } from './enable/enable-user.repository.js';
import { UmbChangeUserPasswordRepository } from './change-password/change-user-password.repository.js';
import { UmbUnlockUserRepository } from './unlock/unlock-user.repository.js';
import { UmbInviteUserRepository } from './invite/invite-user.repository.js';
import type { ManifestStore, ManifestRepository, ManifestItemStore } from '@umbraco-cms/backoffice/extension-registry';

export const USER_REPOSITORY_ALIAS = 'Umb.Repository.User';
const repository: ManifestRepository = {
	type: 'repository',
	alias: USER_REPOSITORY_ALIAS,
	name: 'User Repository',
	api: UmbUserRepository,
};

export const DISABLE_USER_REPOSITORY_ALIAS = 'Umb.Repository.User.Disable';
const disableRepository: ManifestRepository = {
	type: 'repository',
	alias: DISABLE_USER_REPOSITORY_ALIAS,
	name: 'Disable User Repository',
	api: UmbDisableUserRepository,
};

export const ENABLE_USER_REPOSITORY_ALIAS = 'Umb.Repository.User.Enable';
const enableRepository: ManifestRepository = {
	type: 'repository',
	alias: ENABLE_USER_REPOSITORY_ALIAS,
	name: 'Disable User Repository',
	api: UmbEnableUserRepository,
};

export const CHANGE_USER_PASSWORD_REPOSITORY_ALIAS = 'Umb.Repository.User.ChangePassword';
const changePasswordRepository: ManifestRepository = {
	type: 'repository',
	alias: CHANGE_USER_PASSWORD_REPOSITORY_ALIAS,
	name: 'Change User Password Repository',
	api: UmbChangeUserPasswordRepository,
};

export const UNLOCK_USER_REPOSITORY_ALIAS = 'Umb.Repository.User.Unlock';
const unlockRepository: ManifestRepository = {
	type: 'repository',
	alias: UNLOCK_USER_REPOSITORY_ALIAS,
	name: 'Unlock User Repository',
	api: UmbUnlockUserRepository,
};

export const INVITE_USER_REPOSITORY_ALIAS = 'Umb.Repository.User.Invite';
const inviteRepository: ManifestRepository = {
	type: 'repository',
	alias: INVITE_USER_REPOSITORY_ALIAS,
	name: 'Invite User Repository',
	api: UmbInviteUserRepository,
};

const store: ManifestStore = {
	type: 'store',
	alias: 'Umb.Store.User',
	name: 'User Store',
	api: UmbUserStore,
};

const itemStore: ManifestItemStore = {
	type: 'itemStore',
	alias: 'Umb.ItemStore.User',
	name: 'User Store',
	api: UmbUserItemStore,
};

export const manifests = [
	repository,
	disableRepository,
	enableRepository,
	changePasswordRepository,
	unlockRepository,
	inviteRepository,
	store,
	itemStore,
];
