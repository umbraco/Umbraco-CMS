import type { ManifestRepository, ManifestStore, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_RELATION_TYPE_DETAIL_REPOSITORY_ALIAS = 'Umb.Repository.RelationType.Detail';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_RELATION_TYPE_DETAIL_REPOSITORY_ALIAS,
	name: 'Relation Type Detail Repository',
	api: () => import('./relation-type-detail.repository.js'),
};

export const UMB_RELATION_TYPE_DETAIL_STORE_ALIAS = 'Umb.Store.RelationType.Detail';

const store: ManifestStore = {
	type: 'store',
	alias: UMB_RELATION_TYPE_DETAIL_STORE_ALIAS,
	name: 'Relation Type Detail Store',
	api: () => import('./relation-type-detail.store.js'),
};

export const manifests: Array<ManifestTypes> = [repository, store];
