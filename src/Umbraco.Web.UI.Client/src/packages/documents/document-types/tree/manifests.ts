import { UMB_DOCUMENT_TYPE_ENTITY_TYPE, UMB_DOCUMENT_TYPE_ROOT_ENTITY_TYPE } from '../entity.js';
import { UMB_DOCUMENT_TYPE_ROOT_WORKSPACE_ALIAS } from '../constants.js';
import {
	UMB_DOCUMENT_TYPE_TREE_ALIAS,
	UMB_DOCUMENT_TYPE_TREE_REPOSITORY_ALIAS,
	UMB_DOCUMENT_TYPE_TREE_STORE_ALIAS,
	UMB_DOCUMENT_TYPE_FOLDER_ENTITY_TYPE,
	UMB_DOCUMENT_TYPE_FOLDER_WORKSPACE_ALIAS,
	UMB_DOCUMENT_TYPE_TREE_ITEM_CHILDREN_COLLECTION_ALIAS,
} from './constants.js';
import { manifests as folderManifests } from './folder/manifests.js';
import { manifests as treeItemChildrenManifests } from './tree-item-children/manifests.js';

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
		type: 'workspaceView',
		kind: 'collection',
		alias: 'Umb.WorkspaceView.DocumentType.TreeItemChildrenCollection',
		name: 'Document Type Tree Item Children Collection Workspace View',
		meta: {
			label: '#general_design',
			pathname: 'design',
			icon: 'icon-member-dashed-line',
			collectionAlias: UMB_DOCUMENT_TYPE_TREE_ITEM_CHILDREN_COLLECTION_ALIAS,
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				oneOf: [UMB_DOCUMENT_TYPE_ROOT_WORKSPACE_ALIAS, UMB_DOCUMENT_TYPE_FOLDER_WORKSPACE_ALIAS],
			},
		],
	},
	...folderManifests,
	...treeItemChildrenManifests,
];
