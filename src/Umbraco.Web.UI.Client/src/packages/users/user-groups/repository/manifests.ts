import { UmbUserGroupRepository } from '../repository/user-group.repository.js';
import { UmbUserGroupStore } from './user-group.store.js';
import type { ManifestStore, ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const USER_GROUP_REPOSITORY_ALIAS = 'Umb.Repository.UserGroup';

const repository: ManifestRepository = {
	type: 'repository',
	alias: USER_GROUP_REPOSITORY_ALIAS,
	name: 'User Group Repository',
	class: UmbUserGroupRepository,
};

export const USER_GROUP_STORE_ALIAS = 'Umb.Store.UserGroup';

const store: ManifestStore = {
	type: 'store',
	alias: USER_GROUP_STORE_ALIAS,
	name: 'User Group Store',
	class: UmbUserGroupStore,
};

export const manifests = [repository, store];
