import { UmbUserRepository } from '../repository/user.repository.js';
import { UmbUserItemStore } from './user-item.store.js';
import { UmbUserStore } from './user.store.js';
import type { ManifestStore, ManifestRepository, ManifestItemStore } from '@umbraco-cms/backoffice/extension-registry';

const repository: ManifestRepository = {
	type: 'repository',
	alias: 'Umb.Repository.User',
	name: 'User Repository',
	class: UmbUserRepository,
};

const store: ManifestStore = {
	type: 'store',
	alias: 'Umb.Store.User',
	name: 'User Store',
	class: UmbUserStore,
};

const itemStore: ManifestItemStore = {
	type: 'itemStore',
	alias: 'Umb.ItemStore.User',
	name: 'User Store',
	class: UmbUserItemStore,
};

export const manifests = [repository, store, itemStore];
