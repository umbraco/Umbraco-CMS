import type { ManifestStore, ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_RELATION_TYPE_REPOSITORY_ALIAS = 'Umb.Repository.RelationType';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_RELATION_TYPE_REPOSITORY_ALIAS,
	name: 'Relation Type Repository',
	api: () => import('./relation-type.repository.js'),
};

export const UMB_RELATION_TYPE_STORE_ALIAS = 'Umb.Store.RelationType';

const store: ManifestStore = {
	type: 'store',
	alias: UMB_RELATION_TYPE_STORE_ALIAS,
	name: 'Relation Type Store',
	api: () => import('./relation-type.store.js'),
};

export const manifests = [repository, store];
