import type { ManifestRepository, ManifestStore } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_PARTIAL_VIEW_DETAIL_REPOSITORY_ALIAS = 'Umb.Repository.PartialView.Detail';
export const UMB_PARTIAL_VIEW_DETAIL_STORE_ALIAS = 'Umb.Store.PartialView.Detail';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_PARTIAL_VIEW_DETAIL_REPOSITORY_ALIAS,
	name: 'Partial View Detail Repository',
	api: () => import('./partial-view-detail.repository.js'),
};

const store: ManifestStore = {
	type: 'store',
	alias: UMB_PARTIAL_VIEW_DETAIL_STORE_ALIAS,
	name: 'Partial View Detail Store',
	api: () => import('./partial-view-detail.store.js'),
};

export const manifests = [repository, store];
