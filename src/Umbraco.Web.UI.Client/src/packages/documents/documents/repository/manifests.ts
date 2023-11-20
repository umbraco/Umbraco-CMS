import { UmbDocumentRepository } from '../repository/document.repository.js';
import { UmbDocumentItemStore } from './document-item.store.js';
import { UmbDocumentStore } from './document.store.js';
import type { ManifestItemStore, ManifestRepository, ManifestStore } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DOCUMENT_REPOSITORY_ALIAS = 'Umb.Repository.Document';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DOCUMENT_REPOSITORY_ALIAS,
	name: 'Document Repository',
	api: UmbDocumentRepository,
};

export const UMB_DOCUMENT_STORE_ALIAS = 'Umb.Store.Document';
export const UMB_DOCUMENT_ITEM_STORE_ALIAS = 'Umb.Store.Document.Item';

const store: ManifestStore = {
	type: 'store',
	alias: UMB_DOCUMENT_STORE_ALIAS,
	name: 'Document Store',
	api: UmbDocumentStore,
};

const itemStore: ManifestItemStore = {
	type: 'itemStore',
	alias: UMB_DOCUMENT_ITEM_STORE_ALIAS,
	name: 'Document Item Store',
	api: UmbDocumentItemStore,
};

export const manifests = [repository, store, itemStore];
