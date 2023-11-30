import { UmbUserRepository } from './user.repository.js';
import { UmbUserStore } from './user.store.js';
import { UmbDisableUserRepository } from './disable/disable-user.repository.js';
import { UmbEnableUserRepository } from './enable/enable-user.repository.js';
import { UmbChangeUserPasswordRepository } from './change-password/change-user-password.repository.js';
import { UmbUnlockUserRepository } from './unlock/unlock-user.repository.js';
import { manifests as itemManifests } from './item/manifests.js';
import type { ManifestStore, ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_USER_REPOSITORY_ALIAS = 'Umb.Repository.User';
const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_USER_REPOSITORY_ALIAS,
	name: 'User Repository',
	api: UmbUserRepository,
};

export const UMB_DISABLE_USER_REPOSITORY_ALIAS = 'Umb.Repository.User.Disable';
const disableRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DISABLE_USER_REPOSITORY_ALIAS,
	name: 'Disable User Repository',
	api: UmbDisableUserRepository,
};

export const UMB_ENABLE_USER_REPOSITORY_ALIAS = 'Umb.Repository.User.Enable';
const enableRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_ENABLE_USER_REPOSITORY_ALIAS,
	name: 'Disable User Repository',
	api: UmbEnableUserRepository,
};

export const UMB_CHANGE_USER_PASSWORD_REPOSITORY_ALIAS = 'Umb.Repository.User.ChangePassword';
const changePasswordRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_CHANGE_USER_PASSWORD_REPOSITORY_ALIAS,
	name: 'Change User Password Repository',
	api: UmbChangeUserPasswordRepository,
};

export const UMB_UNLOCK_USER_REPOSITORY_ALIAS = 'Umb.Repository.User.Unlock';
const unlockRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_UNLOCK_USER_REPOSITORY_ALIAS,
	name: 'Unlock User Repository',
	api: UmbUnlockUserRepository,
};

const store: ManifestStore = {
	type: 'store',
	alias: 'Umb.Store.User',
	name: 'User Store',
	api: UmbUserStore,
};

export const manifests = [
	repository,
	disableRepository,
	enableRepository,
	changePasswordRepository,
	unlockRepository,
	store,
	...itemManifests,
];
