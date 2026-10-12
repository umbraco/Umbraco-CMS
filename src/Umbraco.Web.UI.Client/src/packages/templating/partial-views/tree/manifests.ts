import {
	UMB_PARTIAL_VIEW_ENTITY_TYPE,
	UMB_PARTIAL_VIEW_FOLDER_ENTITY_TYPE,
	UMB_PARTIAL_VIEW_ROOT_ENTITY_TYPE,
} from '../entity.js';
import { manifests as folderManifests } from './folder/manifests.js';
import { UMB_PARTIAL_VIEW_FOLDER_WORKSPACE_ALIAS } from './folder/workspace/constants.js';
import { manifests as reloadTreeItemChildrenManifest } from './reload-tree-item-children/manifests.js';
import { manifests as treeItemChildrenManifests } from './tree-item-children/manifests.js';
import { manifests as viewManifests } from './views/manifests.js';
import {
	UMB_PARTIAL_VIEW_TREE_ALIAS,
	UMB_PARTIAL_VIEW_TREE_REPOSITORY_ALIAS,
	UMB_PARTIAL_VIEW_TREE_STORE_ALIAS,
} from './constants.js';
import { UmbPartialViewTreeStore } from './partial-view-tree.store.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';
import { UMB_TREE_ALIAS_CONDITION } from '@umbraco-cms/backoffice/tree';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export { UMB_PARTIAL_VIEW_TREE_REPOSITORY_ALIAS, UMB_PARTIAL_VIEW_TREE_STORE_ALIAS, UMB_PARTIAL_VIEW_TREE_ALIAS };

const UMB_PARTIAL_VIEW_ROOT_WORKSPACE_ALIAS = 'Umb.Workspace.PartialView.Root';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'repository',
		alias: UMB_PARTIAL_VIEW_TREE_REPOSITORY_ALIAS,
		name: 'Partial View Tree Repository',
		api: () => import('./partial-view-tree.repository.js'),
	},
	{
		type: 'treeStore',
		alias: UMB_PARTIAL_VIEW_TREE_STORE_ALIAS,
		name: 'Partial View Tree Store',
		api: UmbPartialViewTreeStore,
	},
	{
		type: 'tree',
		kind: 'default',
		alias: UMB_PARTIAL_VIEW_TREE_ALIAS,
		name: 'Partial View Tree',
		meta: {
			repositoryAlias: UMB_PARTIAL_VIEW_TREE_REPOSITORY_ALIAS,
		},
	},
	{
		type: 'treeItem',
		kind: 'default',
		alias: 'Umb.TreeItem.PartialView',
		name: 'Partial View Tree Item',
		forEntityTypes: [
			UMB_PARTIAL_VIEW_ROOT_ENTITY_TYPE,
			UMB_PARTIAL_VIEW_ENTITY_TYPE,
			UMB_PARTIAL_VIEW_FOLDER_ENTITY_TYPE,
		],
	},
	{
		type: 'workspace',
		kind: 'default',
		alias: UMB_PARTIAL_VIEW_ROOT_WORKSPACE_ALIAS,
		name: 'Partial View Root Workspace',
		meta: {
			entityType: UMB_PARTIAL_VIEW_ROOT_ENTITY_TYPE,
			headline: '#treeHeaders_partialViews',
		},
	},
	{
		type: 'treeAction',
		kind: 'create',
		name: 'Partial View Tree Create Action',
		alias: 'Umb.TreeAction.PartialView.Create',
		conditions: [
			{
				alias: UMB_TREE_ALIAS_CONDITION,
				match: UMB_PARTIAL_VIEW_TREE_ALIAS,
			},
		],
	},
	{
		type: 'workspaceView',
		kind: 'tree',
		// TODO (V20): rename alias to 'Umb.WorkspaceView.PartialView.Tree' — kept as the old
		// TreeItemChildrenCollection alias so existing plugin conditions/overrides don't break.
		alias: 'Umb.WorkspaceView.PartialView.TreeItemChildrenCollection',
		name: 'Partial View Tree Item Children Workspace View',
		meta: {
			label: '#tree_children',
			pathname: 'children',
			icon: 'icon-bulleted-list',
			treeAlias: UMB_PARTIAL_VIEW_TREE_ALIAS,
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				oneOf: [UMB_PARTIAL_VIEW_ROOT_WORKSPACE_ALIAS, UMB_PARTIAL_VIEW_FOLDER_WORKSPACE_ALIAS],
			},
		],
	},
	...folderManifests,
	...reloadTreeItemChildrenManifest,
	...treeItemChildrenManifests,
	...viewManifests,
];
