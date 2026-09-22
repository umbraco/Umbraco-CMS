import { UMB_SCRIPT_ENTITY_TYPE, UMB_SCRIPT_FOLDER_ENTITY_TYPE, UMB_SCRIPT_ROOT_ENTITY_TYPE } from '../entity.js';
import { manifests as folderManifests } from './folder/manifests.js';
import { UMB_SCRIPT_FOLDER_WORKSPACE_ALIAS } from './folder/workspace/constants.js';
import { manifests as reloadTreeItemChildrenManifest } from './reload-tree-item-children/manifests.js';
import { manifests as treeItemChildrenManifests } from './tree-item-children/manifests.js';
import { manifests as viewManifests } from './views/manifests.js';
import { UMB_SCRIPT_TREE_ALIAS, UMB_SCRIPT_TREE_REPOSITORY_ALIAS, UMB_SCRIPT_TREE_STORE_ALIAS } from './constants.js';
import { UmbScriptTreeStore } from './script-tree.store.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';
import { UMB_TREE_ALIAS_CONDITION } from '@umbraco-cms/backoffice/tree';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export { UMB_SCRIPT_TREE_REPOSITORY_ALIAS, UMB_SCRIPT_TREE_STORE_ALIAS, UMB_SCRIPT_TREE_ALIAS };

const UMB_SCRIPT_ROOT_WORKSPACE_ALIAS = 'Umb.Workspace.Script.Root';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'repository',
		alias: UMB_SCRIPT_TREE_REPOSITORY_ALIAS,
		name: 'Script Tree Repository',
		api: () => import('./script-tree.repository.js'),
	},
	{
		type: 'treeStore',
		alias: UMB_SCRIPT_TREE_STORE_ALIAS,
		name: 'Script Tree Store',
		api: UmbScriptTreeStore,
	},
	{
		type: 'tree',
		kind: 'default',
		alias: UMB_SCRIPT_TREE_ALIAS,
		name: 'Script Tree',
		meta: {
			repositoryAlias: UMB_SCRIPT_TREE_REPOSITORY_ALIAS,
		},
	},
	{
		type: 'treeItem',
		kind: 'default',
		alias: 'Umb.TreeItem.Script',
		name: 'Script Tree Item',
		forEntityTypes: [UMB_SCRIPT_ROOT_ENTITY_TYPE, UMB_SCRIPT_ENTITY_TYPE, UMB_SCRIPT_FOLDER_ENTITY_TYPE],
	},
	{
		type: 'workspace',
		kind: 'default',
		alias: UMB_SCRIPT_ROOT_WORKSPACE_ALIAS,
		name: 'Script Root Workspace',
		meta: {
			entityType: UMB_SCRIPT_ROOT_ENTITY_TYPE,
			headline: '#treeHeaders_scripts',
		},
	},
	{
		type: 'treeAction',
		kind: 'create',
		name: 'Script Tree Create Action',
		alias: 'Umb.TreeAction.Script.Create',
		conditions: [
			{
				alias: UMB_TREE_ALIAS_CONDITION,
				match: UMB_SCRIPT_TREE_ALIAS,
			},
		],
	},
	{
		type: 'workspaceView',
		kind: 'tree',
		alias: 'Umb.WorkspaceView.Script.Tree',
		name: 'Script Tree Item Children Workspace View',
		meta: {
			label: '#tree_children',
			pathname: 'children',
			icon: 'icon-bulleted-list',
			treeAlias: UMB_SCRIPT_TREE_ALIAS,
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				oneOf: [UMB_SCRIPT_ROOT_WORKSPACE_ALIAS, UMB_SCRIPT_FOLDER_WORKSPACE_ALIAS],
			},
		],
	},
	...folderManifests,
	...reloadTreeItemChildrenManifest,
	...treeItemChildrenManifests,
	...viewManifests,
];
