import { UMB_DOCUMENT_TYPE_ENTITY_TYPE, UMB_DOCUMENT_TYPE_ROOT_ENTITY_TYPE } from '../entity.js';
import { UMB_DOCUMENT_TYPE_ROOT_WORKSPACE_ALIAS } from '../constants.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import {
	UMB_DOCUMENT_TYPE_TREE_ALIAS,
	UMB_DOCUMENT_TYPE_TREE_REPOSITORY_ALIAS,
	UMB_DOCUMENT_TYPE_TREE_STORE_ALIAS,
	UMB_DOCUMENT_TYPE_FOLDER_ENTITY_TYPE,
	UMB_DOCUMENT_TYPE_FOLDER_WORKSPACE_ALIAS,
} from './constants.js';
import { UmbDocumentTypeTreeStore } from './document-type.tree.store.js';
import { manifests as folderManifests } from './folder/manifests.js';
import { manifests as treeItemChildrenManifests } from './tree-item-children/manifests.js';
import { manifests as viewManifests } from './views/manifests.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';
import { UMB_TREE_ALIAS_CONDITION } from '@umbraco-cms/backoffice/tree';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
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
		api: UmbDocumentTypeTreeStore,
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
		type: 'treeItemCard',
		kind: 'default',
		alias: 'Umb.TreeItemCard.DocumentType',
		name: 'Document Type Tree Item Card',
		forEntityTypes: [
			UMB_DOCUMENT_TYPE_ROOT_ENTITY_TYPE,
			UMB_DOCUMENT_TYPE_ENTITY_TYPE,
			UMB_DOCUMENT_TYPE_FOLDER_ENTITY_TYPE,
		],
	},
	{
		type: 'workspaceView',
		kind: 'tree',
		alias: 'Umb.WorkspaceView.DocumentType.Tree',
		name: 'Document Type Tree Item Children Workspace View',
		meta: {
			label: '#tree_children',
			pathname: 'children',
			icon: 'icon-bulleted-list',
			treeAlias: UMB_DOCUMENT_TYPE_TREE_ALIAS,
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				oneOf: [UMB_DOCUMENT_TYPE_ROOT_WORKSPACE_ALIAS, UMB_DOCUMENT_TYPE_FOLDER_WORKSPACE_ALIAS],
			},
		],
	},
	{
		type: 'treeAction',
		kind: 'create',
		name: 'Document Type Tree Create Action',
		alias: 'Umb.TreeAction.DocumentType.Create',
		conditions: [
			{
				alias: UMB_TREE_ALIAS_CONDITION,
				match: UMB_DOCUMENT_TYPE_TREE_ALIAS,
			},
		],
	},
	...viewManifests,
	...folderManifests,
	...treeItemChildrenManifests,
];
