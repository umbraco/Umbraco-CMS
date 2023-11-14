import { UmbDocumentTypeItemStore } from './document-type-item.store.js';
import { UmbDocumentTypeRepository } from './document-type.repository.js';
import { UmbDocumentTypeStore } from './document-type.store.js';
import { ManifestItemStore, ManifestRepository, ManifestStore } from '@umbraco-cms/backoffice/extension-registry';

export const DOCUMENT_TYPE_REPOSITORY_ALIAS = 'Umb.Repository.DocumentType';

const repository: ManifestRepository = {
	type: 'repository',
	alias: DOCUMENT_TYPE_REPOSITORY_ALIAS,
	name: 'Document Types Repository',
	api: UmbDocumentTypeRepository,
};

export const DOCUMENT_TYPE_STORE_ALIAS = 'Umb.Store.DocumentType';
export const DOCUMENT_TYPE_ITEM_STORE_ALIAS = 'Umb.Store.DocumentTypeItem';

const store: ManifestStore = {
	type: 'store',
	alias: DOCUMENT_TYPE_STORE_ALIAS,
	name: 'Document Type Store',
	api: UmbDocumentTypeStore,
};

const itemStore: ManifestItemStore = {
	type: 'itemStore',
	alias: DOCUMENT_TYPE_ITEM_STORE_ALIAS,
	name: 'Document Type Item Store',
	api: UmbDocumentTypeItemStore,
};

export const manifests = [repository, store, itemStore];
