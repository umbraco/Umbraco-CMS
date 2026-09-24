import { UMB_DATA_TYPE_ROOT_WORKSPACE_ALIAS } from '../data-type-root/index.js';
import { manifests as folderManifests } from './folder/manifests.js';
import { manifests as treeItemChildren } from './tree-item-children/manifests.js';
import { manifests as viewManifests } from './views/manifests.js';
import { UMB_DATA_TYPE_FOLDER_WORKSPACE_ALIAS } from './folder/workspace/index.js';
import {
	UMB_DATA_TYPE_TREE_ALIAS,
	UMB_DATA_TYPE_TREE_REPOSITORY_ALIAS,
	UMB_DATA_TYPE_TREE_STORE_ALIAS,
} from './constants.js';
import { UmbDataTypeTreeStore } from './data-type-tree.store.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';
import { UMB_TREE_ALIAS_CONDITION } from '@umbraco-cms/backoffice/tree';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'repository',
		alias: UMB_DATA_TYPE_TREE_REPOSITORY_ALIAS,
		name: 'Data Type Tree Repository',
		api: () => import('./data-type-tree.repository.js'),
	},
	{
		type: 'treeStore',
		alias: UMB_DATA_TYPE_TREE_STORE_ALIAS,
		name: 'Data Type Tree Store',
		api: UmbDataTypeTreeStore,
	},
	{
		type: 'tree',
		kind: 'default',
		alias: UMB_DATA_TYPE_TREE_ALIAS,
		name: 'Data Types Tree',
		meta: {
			repositoryAlias: UMB_DATA_TYPE_TREE_REPOSITORY_ALIAS,
		},
	},
	{
		type: 'treeItem',
		kind: 'default',
		alias: 'Umb.TreeItem.DataType',
		name: 'Data Type Tree Item',
		forEntityTypes: ['data-type-root', 'data-type', 'data-type-folder'],
	},
	{
		type: 'treeAction',
		kind: 'create',
		name: 'Data Type Tree Create Action',
		alias: 'Umb.TreeAction.DataType.Create',
		conditions: [
			{
				alias: UMB_TREE_ALIAS_CONDITION,
				match: UMB_DATA_TYPE_TREE_ALIAS,
			},
		],
	},
	{
		type: 'workspaceView',
		kind: 'tree',
		// TODO (V20): rename alias to 'Umb.WorkspaceView.DataType.Tree' — kept as the old
		// TreeItemChildrenCollection alias so existing plugin conditions/overrides don't break.
		alias: 'Umb.WorkspaceView.DataType.TreeItemChildrenCollection',
		name: 'Data Type Tree Item Children Workspace View',
		meta: {
			label: '#tree_children',
			pathname: 'children',
			icon: 'icon-bulleted-list',
			treeAlias: UMB_DATA_TYPE_TREE_ALIAS,
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				oneOf: [UMB_DATA_TYPE_ROOT_WORKSPACE_ALIAS, UMB_DATA_TYPE_FOLDER_WORKSPACE_ALIAS],
			},
		],
	},
	...folderManifests,
	...treeItemChildren,
	...viewManifests,
];
