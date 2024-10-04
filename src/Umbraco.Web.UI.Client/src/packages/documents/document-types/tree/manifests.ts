import {
	UMB_DOCUMENT_TYPE_ENTITY_TYPE,
	UMB_DOCUMENT_TYPE_FOLDER_ENTITY_TYPE,
	UMB_DOCUMENT_TYPE_ROOT_ENTITY_TYPE,
} from '../entity.js';
import {
	UMB_DOCUMENT_TYPE_TREE_ALIAS,
	UMB_DOCUMENT_TYPE_TREE_REPOSITORY_ALIAS,
	UMB_DOCUMENT_TYPE_TREE_STORE_ALIAS,
} from './constants.js';
import { manifests as folderManifests } from './folder/manifests.js';
import { manifests as reloadManifests } from './reload-tree-item-children/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DOCUMENT_TYPE_TREE_REPOSITORY_ALIAS,
		name: 'Document Type Tree Repository',
		api: () => import('./document-type-tree.repository.js'),
	},
	{
		type: 'treeStore',
		alias: UMB_DOCUMENT_TYPE_TREE_STORE_ALIAS,
		name: 'Document Type Tree Store',
		api: () => import('./document-type.tree.store.js'),
	},
	{
		type: 'tree',
		kind: 'default',
		alias: UMB_DOCUMENT_TYPE_TREE_ALIAS,
		name: 'Document Type Tree',
		meta: {
			repositoryAlias: UMB_DOCUMENT_TYPE_TREE_REPOSITORY_ALIAS,
		},
	},
	{
		type: 'treeItem',
		kind: 'default',
		alias: 'Umb.TreeItem.DocumentType',
		name: 'Document Type Tree Item',
		forEntityTypes: [
			UMB_DOCUMENT_TYPE_ROOT_ENTITY_TYPE,
			UMB_DOCUMENT_TYPE_ENTITY_TYPE,
			UMB_DOCUMENT_TYPE_FOLDER_ENTITY_TYPE,
		],
	},
	{
		type: 'workspace',
		kind: 'default',
		alias: 'Umb.Workspace.DocumentType.Root',
		name: 'Document Type Root Workspace',
		meta: {
			entityType: UMB_DOCUMENT_TYPE_ROOT_ENTITY_TYPE,
			headline: '#treeHeaders_documentTypes',
		},
	},
	...folderManifests,
	...reloadManifests,
];
