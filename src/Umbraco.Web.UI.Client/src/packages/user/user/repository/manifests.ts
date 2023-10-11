import { UmbUserRepository } from './user.repository.js';
import { UmbUserItemStore } from './user-item.store.js';
import { UmbUserStore } from './user.store.js';
import { UmbDisableUserRepository } from './disable-user.repository.js';
import type { ManifestStore, ManifestRepository, ManifestItemStore } from '@umbraco-cms/backoffice/extension-registry';

export const USER_REPOSITORY_ALIAS = 'Umb.Repository.User';
export const DISABLE_USER_REPOSITORY_ALIAS = 'Umb.Repository.User.Disable';

const repository: ManifestRepository = {
	type: 'repository',
	alias: USER_REPOSITORY_ALIAS,
	name: 'User Repository',
	api: UmbUserRepository,
};

const disableRepository: ManifestRepository = {
	type: 'repository',
	alias: DISABLE_USER_REPOSITORY_ALIAS,
	name: 'Disable User Repository',
	api: UmbDisableUserRepository,
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

export const manifests = [repository, disableRepository, store, itemStore];
