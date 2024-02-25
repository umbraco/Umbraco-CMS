import type { ManifestStore, ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_TAG_REPOSITORY_ALIAS = 'Umb.Repository.Tags';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_TAG_REPOSITORY_ALIAS,
	name: 'Tags Repository',
	api: () => import('./tag.repository.js'),
};

export const UMB_TAG_STORE_ALIAS = 'Umb.Store.Tags';

const store: ManifestStore = {
	type: 'store',
	alias: UMB_TAG_STORE_ALIAS,
	name: 'Tags Store',
	api: () => import('./tag.store.js'),
};

export const manifests = [repository, store];
