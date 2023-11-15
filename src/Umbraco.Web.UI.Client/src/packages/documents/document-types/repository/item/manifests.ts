import { UmbDocumentTypeItemRepository } from './document-type-item.repository.js';
import { UmbDocumentTypeItemStore } from './document-type-item.store.js';
import { ManifestItemStore, ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const DOCUMENT_TYPE_ITEM_REPOSITORY_ALIAS = 'Umb.Repository.DocumentType.Item';
export const DOCUMENT_TYPE_ITEM_STORE_ALIAS = 'Umb.Store.DocumentType.Item';

const itemRepository: ManifestRepository = {
	type: 'repository',
	alias: DOCUMENT_TYPE_ITEM_REPOSITORY_ALIAS,
	name: 'Document Type Item Repository',
	api: UmbDocumentTypeItemRepository,
};

const itemStore: ManifestItemStore = {
	type: 'itemStore',
	alias: DOCUMENT_TYPE_ITEM_STORE_ALIAS,
	name: 'Document Type Item Store',
	api: UmbDocumentTypeItemStore,
};

export const manifests = [itemRepository, itemStore];
