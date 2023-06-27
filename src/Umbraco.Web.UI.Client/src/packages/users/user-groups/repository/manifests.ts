import { UmbUserGroupRepository } from '../repository/user-group.repository.js';
import { UmbUserGroupItemStore } from './user-group-item.store.js';
import { UmbUserGroupStore } from './user-group.store.js';
import type { ManifestStore, ManifestRepository, ManifestItemStore } from '@umbraco-cms/backoffice/extension-registry';

const repository: ManifestRepository = {
	type: 'repository',
	alias: 'Umb.Repository.UserGroup',
	name: 'User Group Repository',
	class: UmbUserGroupRepository,
};

const store: ManifestStore = {
	type: 'store',
	alias: 'Umb.Store.UserGroup',
	name: 'User Group Store',
	class: UmbUserGroupStore,
};

const itemStore: ManifestItemStore = {
	type: 'itemStore',
	alias: 'Umb.ItemStore.UserGroup',
	name: 'User Group Item Store',
	class: UmbUserGroupItemStore,
};

export const manifests = [repository, store, itemStore];
