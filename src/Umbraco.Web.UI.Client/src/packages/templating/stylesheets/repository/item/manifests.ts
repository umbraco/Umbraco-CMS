import type { ManifestRepository, ManifestItemStore } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_STYLESHEET_ITEM_REPOSITORY_ALIAS = 'Umb.Repository.Stylesheet.Item';
export const UMB_STYLESHEET_ITEM_STORE_ALIAS = 'Umb.ItemStore.Stylesheet';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_STYLESHEET_ITEM_REPOSITORY_ALIAS,
	name: 'Stylesheet Item Repository',
	api: () => import('./stylesheet-item.repository.js'),
};

const itemStore: ManifestItemStore = {
	type: 'itemStore',
	alias: 'Umb.ItemStore.Stylesheet',
	name: 'Stylesheet Item Store',
	api: () => import('./stylesheet-item.store.js'),
};

export const manifests = [repository, itemStore];
