import {
	UMB_PARTIAL_VIEW_ENTITY_TYPE,
	UMB_PARTIAL_VIEW_FOLDER_ENTITY_TYPE,
	UMB_PARTIAL_VIEW_ROOT_ENTITY_TYPE,
} from '../entity.js';
import { UMB_PARTIAL_VIEW_TREE_ITEM_CHILDREN_COLLECTION_ALIAS } from './tree-item-children/constants.js';
import { manifests as folderManifests } from './folder/manifests.js';
import { manifests as reloadTreeItemChildrenManifest } from './reload-tree-item-children/manifests.js';
import { manifests as treeItemChildrenManifests } from './tree-item-children/manifests.js';
import { UmbPartialViewTreeStore } from './partial-view-tree.store.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const UMB_PARTIAL_VIEW_TREE_REPOSITORY_ALIAS = 'Umb.Repository.PartialView.Tree';
/**
 * @deprecated Use {@link UMB_PARTIAL_VIEW_TREE_REPOSITORY_ALIAS} instead. This will be removed in Umbraco 18.
 */
export const UMB_PARTIAL_VIEW_TREE_STORE_ALIAS = 'Umb.Store.PartialView.Tree';
export const UMB_PARTIAL_VIEW_TREE_ALIAS = 'Umb.Tree.PartialView';

const UMB_PARTIAL_VIEW_ROOT_WORKSPACE_ALIAS = 'Umb.Workspace.PartialView.Root';

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
		type: 'workspaceView',
		kind: 'collection',
		alias: 'Umb.WorkspaceView.PartialView.TreeItemChildrenCollection',
		name: 'Partial View Tree Item Children Collection Workspace View',
		meta: {
			label: '#general_items',
			pathname: 'items',
			icon: 'icon-grid',
			collectionAlias: UMB_PARTIAL_VIEW_TREE_ITEM_CHILDREN_COLLECTION_ALIAS,
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				oneOf: [UMB_PARTIAL_VIEW_ROOT_WORKSPACE_ALIAS, 'Umb.Workspace.PartialView.Folder'],
			},
		],
	},
	...folderManifests,
	...reloadTreeItemChildrenManifest,
	...treeItemChildrenManifests,
];
