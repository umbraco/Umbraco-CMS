import { UmbUserGroupRepository } from '../repository/user-group.repository';
import { UmbUserGroupStore } from './user-group.store';
import type { ManifestStore, ManifestRepository } from '@umbraco-cms/extensions-registry';

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
