import {
	UMB_STYLESHEET_ENTITY_TYPE,
	UMB_STYLESHEET_FOLDER_ENTITY_TYPE,
	UMB_STYLESHEET_ROOT_ENTITY_TYPE,
} from '../entity.js';
import { manifests as folderManifests } from './folder/manifests.js';
import { UMB_STYLESHEET_FOLDER_WORKSPACE_ALIAS } from './folder/workspace/constants.js';
import { manifests as reloadTreeItemChildrenManifest } from './reload-tree-item-children/manifests.js';
import { manifests as treeItemChildrenManifests } from './tree-item-children/manifests.js';
import { manifests as viewManifests } from './views/manifests.js';
import {
	UMB_STYLESHEET_TREE_ALIAS,
	UMB_STYLESHEET_TREE_REPOSITORY_ALIAS,
	UMB_STYLESHEET_TREE_STORE_ALIAS,
} from './constants.js';
import { UmbStylesheetTreeStore } from './stylesheet-tree.store.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';
import { UMB_TREE_ALIAS_CONDITION } from '@umbraco-cms/backoffice/tree';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export { UMB_STYLESHEET_TREE_REPOSITORY_ALIAS, UMB_STYLESHEET_TREE_STORE_ALIAS, UMB_STYLESHEET_TREE_ALIAS };

const UMB_STYLESHEET_ROOT_WORKSPACE_ALIAS = 'Umb.Workspace.Stylesheet.Root';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'repository',
		alias: UMB_STYLESHEET_TREE_REPOSITORY_ALIAS,
		name: 'Stylesheet Tree Repository',
		api: () => import('./stylesheet-tree.repository.js'),
	},
	{
		type: 'treeStore',
		alias: UMB_STYLESHEET_TREE_STORE_ALIAS,
		name: 'Stylesheet Tree Store',
		api: UmbStylesheetTreeStore,
	},
	{
		type: 'tree',
		kind: 'default',
		alias: UMB_STYLESHEET_TREE_ALIAS,
		name: 'Stylesheet Tree',
		weight: 10,
		meta: {
			repositoryAlias: UMB_STYLESHEET_TREE_REPOSITORY_ALIAS,
		},
	},
	{
		type: 'treeItem',
		kind: 'default',
		alias: 'Umb.TreeItem.Stylesheet',
		name: 'Stylesheet Tree Item',
		forEntityTypes: [UMB_STYLESHEET_ROOT_ENTITY_TYPE, UMB_STYLESHEET_ENTITY_TYPE, UMB_STYLESHEET_FOLDER_ENTITY_TYPE],
	},
	{
		type: 'workspace',
		kind: 'default',
		alias: UMB_STYLESHEET_ROOT_WORKSPACE_ALIAS,
		name: 'Stylesheet Root Workspace',
		meta: {
			entityType: UMB_STYLESHEET_ROOT_ENTITY_TYPE,
			headline: '#treeHeaders_stylesheets',
		},
	},
	{
		type: 'treeAction',
		kind: 'create',
		name: 'Stylesheet Tree Create Action',
		alias: 'Umb.TreeAction.Stylesheet.Create',
		conditions: [
			{
				alias: UMB_TREE_ALIAS_CONDITION,
				match: UMB_STYLESHEET_TREE_ALIAS,
			},
		],
	},
	{
		type: 'workspaceView',
		kind: 'tree',
		// TODO (V20): rename alias to 'Umb.WorkspaceView.Stylesheet.Tree' — kept as the old
		// TreeItemChildrenCollection alias so existing plugin conditions/overrides don't break.
		alias: 'Umb.WorkspaceView.Stylesheet.TreeItemChildrenCollection',
		name: 'Stylesheet Tree Item Children Workspace View',
		meta: {
			label: '#tree_children',
			pathname: 'children',
			icon: 'icon-bulleted-list',
			treeAlias: UMB_STYLESHEET_TREE_ALIAS,
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				oneOf: [UMB_STYLESHEET_ROOT_WORKSPACE_ALIAS, UMB_STYLESHEET_FOLDER_WORKSPACE_ALIAS],
			},
		],
	},
	...folderManifests,
	...reloadTreeItemChildrenManifest,
	...treeItemChildrenManifests,
	...viewManifests,
];
