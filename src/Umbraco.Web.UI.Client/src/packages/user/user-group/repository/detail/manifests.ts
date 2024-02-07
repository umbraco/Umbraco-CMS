import { UmbUserGroupDetailRepository } from './user-group-detail.repository.js';
import { UmbUserGroupDetailStore } from './user-group-detail.store.js';
import type { ManifestRepository, ManifestStore } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_USER_GROUP_DETAIL_REPOSITORY_ALIAS = 'Umb.Repository.UserGroup.Detail';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_USER_GROUP_DETAIL_REPOSITORY_ALIAS,
	name: 'User Group Detail Repository',
	api: UmbUserGroupDetailRepository,
};

export const UMB_USER_GROUP_DETAIL_STORE_ALIAS = 'Umb.Store.UserGroup.Detail';

const store: ManifestStore = {
	type: 'store',
	alias: UMB_USER_GROUP_DETAIL_STORE_ALIAS,
	name: 'User Group Detail Store',
	api: UmbUserGroupDetailStore,
};

export const manifests = [repository, store];
