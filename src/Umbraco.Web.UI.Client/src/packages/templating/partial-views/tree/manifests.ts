import {
	UMB_PARTIAL_VIEW_ENTITY_TYPE,
	UMB_PARTIAL_VIEW_FOLDER_ENTITY_TYPE,
	UMB_PARTIAL_VIEW_ROOT_ENTITY_TYPE,
} from '../entity.js';
import { manifests as folderManifests } from './folder/manifests.js';
import { manifests as reloadTreeItemChildrenManifest } from './reload-tree-item-children/manifests.js';

export const UMB_PARTIAL_VIEW_TREE_REPOSITORY_ALIAS = 'Umb.Repository.PartialView.Tree';
export const UMB_PARTIAL_VIEW_TREE_STORE_ALIAS = 'Umb.Store.PartialView.Tree';
export const UMB_PARTIAL_VIEW_TREE_ALIAS = 'Umb.Tree.PartialView';

export const manifests: Array<UmbExtensionManifest> = [
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
		api: () => import('./partial-view-tree.store.js'),
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
		alias: 'Umb.Workspace.PartialView.Root',
		name: 'Partial View Root Workspace',
		meta: {
			entityType: UMB_PARTIAL_VIEW_ROOT_ENTITY_TYPE,
			headline: '#treeHeaders_partialViews',
		},
	},
	...folderManifests,
	...reloadTreeItemChildrenManifest,
];
