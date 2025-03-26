import {
	UMB_DOCUMENT_RECYCLE_BIN_TREE_REPOSITORY_ALIAS,
	UMB_DOCUMENT_RECYCLE_BIN_TREE_STORE_ALIAS,
} from '../constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DOCUMENT_RECYCLE_BIN_TREE_REPOSITORY_ALIAS,
		name: 'Document Recycle Bin Tree Repository',
		api: () => import('./document-recycle-bin-tree.repository.js'),
	},
	{
		type: 'treeStore',
		alias: UMB_DOCUMENT_RECYCLE_BIN_TREE_STORE_ALIAS,
		name: 'Document Recycle Bin Tree Store',
		api: () => import('./document-recycle-bin-tree.store.js'),
	},
];
