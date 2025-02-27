import { UMB_DOCUMENT_RECYCLE_BIN_ROOT_ENTITY_TYPE } from '../constants.js';
import {
	UMB_DOCUMENT_RECYCLE_BIN_TREE_ALIAS,
	UMB_DOCUMENT_RECYCLE_BIN_TREE_REPOSITORY_ALIAS,
	UMB_DOCUMENT_RECYCLE_BIN_TREE_STORE_ALIAS,
} from './constants.js';
import { manifests as reloadTreeItemChildrenManifests } from './reload-tree-item-children/manifests.js';

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
	{
		type: 'tree',
		kind: 'default',
		alias: UMB_DOCUMENT_RECYCLE_BIN_TREE_ALIAS,
		name: 'Document Recycle Bin Tree',
		meta: {
			repositoryAlias: UMB_DOCUMENT_RECYCLE_BIN_TREE_REPOSITORY_ALIAS,
		},
	},
	{
		type: 'treeItem',
		kind: 'default',
		alias: 'Umb.TreeItem.Document.RecycleBin',
		name: 'Document Recycle Bin Tree Item',
		forEntityTypes: [UMB_DOCUMENT_RECYCLE_BIN_ROOT_ENTITY_TYPE],
	},
	{
		type: 'workspace',
		kind: 'default',
		alias: 'Umb.Workspace.Document.RecycleBin.Root',
		name: 'Document Recycle Bin Root Workspace',
		meta: {
			entityType: UMB_DOCUMENT_RECYCLE_BIN_ROOT_ENTITY_TYPE,
			headline: '#general_recycleBin',
		},
	},
	...reloadTreeItemChildrenManifests,
];
