import { UMB_SCRIPT_ENTITY_TYPE, UMB_SCRIPT_FOLDER_ENTITY_TYPE, UMB_SCRIPT_ROOT_ENTITY_TYPE } from '../entity.js';
import { manifests as folderManifests } from './folder/manifests.js';
import { manifests as reloadTreeItemChildrenManifest } from './reload-tree-item-children/manifests.js';

export const UMB_SCRIPT_TREE_REPOSITORY_ALIAS = 'Umb.Repository.Script.Tree';
export const UMB_SCRIPT_TREE_STORE_ALIAS = 'Umb.Store.Script.Tree';
export const UMB_SCRIPT_TREE_ALIAS = 'Umb.Tree.Script';

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
		api: () => import('./script-tree.store.js'),
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
		alias: 'Umb.Workspace.Script.Root',
		name: 'Script Root Workspace',
		meta: {
			entityType: UMB_SCRIPT_ROOT_ENTITY_TYPE,
			headline: '#treeHeaders_scripts',
		},
	},
	...folderManifests,
	...reloadTreeItemChildrenManifest,
];
