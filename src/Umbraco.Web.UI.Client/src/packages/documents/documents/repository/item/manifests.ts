import type { ManifestRepository, ManifestItemStore, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS = 'Umb.Repository.DocumentItem';
export const UMB_DOCUMENT_STORE_ALIAS = 'Umb.Store.DocumentItem';

const itemRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS,
	name: 'Document Item Repository',
	api: () => import('./document-item.repository.js'),
};

const itemStore: ManifestItemStore = {
	type: 'itemStore',
	alias: UMB_DOCUMENT_STORE_ALIAS,
	name: 'Document Item Store',
	api: () => import('./document-item.store.js'),
};

export const manifests: Array<ManifestTypes> = [itemRepository, itemStore];
