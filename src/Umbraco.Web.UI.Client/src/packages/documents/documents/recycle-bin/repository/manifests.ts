import { UmbDocumentRecycleBinRepository } from './document-recycle-bin.repository.js';
import { UmbDocumentRecycleBinTreeStore } from './document-recycle.bin.tree.store.js';
import type { ManifestRepository, ManifestTreeStore } from '@umbraco-cms/backoffice/extension-registry';

export const DOCUMENT_RECYCLE_BIN_REPOSITORY_ALIAS = 'Umb.Repository.DocumentRecycleBin';

const repository: ManifestRepository = {
	type: 'repository',
	alias: DOCUMENT_RECYCLE_BIN_REPOSITORY_ALIAS,
	name: 'Document Recycle Bin Repository',
	api: UmbDocumentRecycleBinRepository,
};

export const DOCUMENT_RECYCLE_BIN_TREE_STORE_ALIAS = 'Umb.Store.DocumentRecycleBinTree';

const treeStore: ManifestTreeStore = {
	type: 'treeStore',
	alias: DOCUMENT_RECYCLE_BIN_TREE_STORE_ALIAS,
	name: 'Document Recycle Bin Tree Store',
	api: UmbDocumentRecycleBinTreeStore,
};

export const manifests = [repository, treeStore];
