import type { ManifestRepository, ManifestStore } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_LANGUAGE_DETAIL_REPOSITORY_ALIAS = 'Umb.Repository.Language.Detail';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_LANGUAGE_DETAIL_REPOSITORY_ALIAS,
	name: 'Language Detail Repository',
	api: () => import('./language-detail.repository.js'),
};

export const UMB_LANGUAGE_DETAIL_STORE_ALIAS = 'Umb.Store.Language.Detail';

const store: ManifestStore = {
	type: 'store',
	alias: UMB_LANGUAGE_DETAIL_STORE_ALIAS,
	name: 'Language Detail Store',
	api: () => import('./language-detail.store.js'),
};

export const manifests = [repository, store];
