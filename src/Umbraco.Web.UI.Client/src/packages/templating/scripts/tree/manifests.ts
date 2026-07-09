import { UMB_SCRIPT_ENTITY_TYPE, UMB_SCRIPT_FOLDER_ENTITY_TYPE, UMB_SCRIPT_ROOT_ENTITY_TYPE } from '../entity.js';
import { UMB_SCRIPT_TREE_ITEM_CHILDREN_COLLECTION_ALIAS } from './tree-item-children/constants.js';
import { manifests as folderManifests } from './folder/manifests.js';
import { manifests as reloadTreeItemChildrenManifest } from './reload-tree-item-children/manifests.js';
import { manifests as treeItemChildrenManifests } from './tree-item-children/manifests.js';
import { UmbScriptTreeStore } from './script-tree.store.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const UMB_SCRIPT_TREE_REPOSITORY_ALIAS = 'Umb.Repository.Script.Tree';
/**
 * @deprecated Use {@link UMB_SCRIPT_TREE_REPOSITORY_ALIAS} instead. This will be removed in Umbraco 18.
 */
export const UMB_SCRIPT_TREE_STORE_ALIAS = 'Umb.Store.Script.Tree';
export const UMB_SCRIPT_TREE_ALIAS = 'Umb.Tree.Script';

const UMB_SCRIPT_ROOT_WORKSPACE_ALIAS = 'Umb.Workspace.Script.Root';

export const manifests: Array<UmbExtensionManifest> = [
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
		type: 'workspaceView',
		kind: 'collection',
		alias: 'Umb.WorkspaceView.Script.TreeItemChildrenCollection',
		name: 'Script Tree Item Children Collection Workspace View',
		meta: {
			label: '#general_items',
			pathname: 'items',
			icon: 'icon-grid',
			collectionAlias: UMB_SCRIPT_TREE_ITEM_CHILDREN_COLLECTION_ALIAS,
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				oneOf: [UMB_SCRIPT_ROOT_WORKSPACE_ALIAS, 'Umb.Workspace.Script.Folder'],
			},
		],
	},
	...folderManifests,
	...reloadTreeItemChildrenManifest,
	...treeItemChildrenManifests,
];
