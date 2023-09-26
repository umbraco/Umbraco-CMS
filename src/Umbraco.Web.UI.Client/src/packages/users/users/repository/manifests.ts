import { UmbUserRepository } from '../repository/user.repository.js';
import { UmbUserItemStore } from './user-item.store.js';
import { UmbUserStore } from './user.store.js';
import type { ManifestStore, ManifestRepository, ManifestItemStore } from '@umbraco-cms/backoffice/extension-registry';

export const USER_REPOSITORY_ALIAS = 'Umb.Repository.User';

const repository: ManifestRepository = {
	type: 'repository',
	alias: USER_REPOSITORY_ALIAS,
	name: 'User Repository',
	api: UmbUserRepository,
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

export const manifests = [repository, store, itemStore];
