import type { ManifestRepository, ManifestItemStore } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_PARTIAL_VIEW_ITEM_REPOSITORY_ALIAS = 'Umb.Repository.PartialView.Item';
export const UMB_PARTIAL_VIEW_ITEM_STORE_ALIAS = 'Umb.ItemStore.PartialView';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_PARTIAL_VIEW_ITEM_REPOSITORY_ALIAS,
	name: 'Partial View Item Repository',
	api: () => import('./partial-view-item.repository.js'),
};

const itemStore: ManifestItemStore = {
	type: 'itemStore',
	alias: 'Umb.ItemStore.PartialView',
	name: 'Partial View Item Store',
	api: () => import('./partial-view-item.store.js'),
};

export const manifests = [repository, itemStore];
