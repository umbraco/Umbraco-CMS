import { UmbDocumentItemStore } from './document-item.store.js';
import { UmbDocumentItemRepository } from './document-item.repository.js';
import type { ManifestRepository, ManifestItemStore } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS = 'Umb.Repository.DocumentItem';
export const UMB_DOCUMENT_STORE_ALIAS = 'Umb.Store.DocumentItem';

const itemRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS,
	name: 'Document Item Repository',
	api: UmbDocumentItemRepository,
};

const itemStore: ManifestItemStore = {
	type: 'itemStore',
	alias: UMB_DOCUMENT_STORE_ALIAS,
	name: 'Document Item Store',
	api: UmbDocumentItemStore,
};

export const manifests = [itemRepository, itemStore];
