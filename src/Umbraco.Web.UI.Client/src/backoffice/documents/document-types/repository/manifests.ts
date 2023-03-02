import { UmbDocumentTypeRepository } from './document-type.repository';
import { UmbDocumentTypeStore } from './document-type.store';
import { UmbDocumentTypeTreeStore } from './document-type.tree.store';
import { ManifestRepository, ManifestStore, ManifestTreeStore } from '@umbraco-cms/extensions-registry';

export const DOCUMENT_TYPE_REPOSITORY_ALIAS = 'Umb.Repository.DocumentType';

const repository: ManifestRepository = {
	type: 'repository',
	alias: DOCUMENT_TYPE_REPOSITORY_ALIAS,
	name: 'Document Types Repository',
	class: UmbDocumentTypeRepository,
};

export const DOCUMENT_TYPE_STORE_ALIAS = 'Umb.Store.DocumentType';
export const DOCUMENT_TYPE_TREE_STORE_ALIAS = 'Umb.Store.DocumentTypeTree';

const store: ManifestStore = {
	type: 'store',
	alias: DOCUMENT_TYPE_STORE_ALIAS,
	name: 'Document Type Store',
	class: UmbDocumentTypeStore,
};

const treeStore: ManifestTreeStore = {
	type: 'treeStore',
	alias: DOCUMENT_TYPE_TREE_STORE_ALIAS,
	name: 'Document Type Tree Store',
	class: UmbDocumentTypeTreeStore,
};

export const manifests = [repository, store, treeStore];
