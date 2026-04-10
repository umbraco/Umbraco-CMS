import {
	UMB_STYLESHEET_ENTITY_TYPE,
	UMB_STYLESHEET_FOLDER_ENTITY_TYPE,
	UMB_STYLESHEET_ROOT_ENTITY_TYPE,
} from '../entity.js';
import { UMB_STYLESHEET_TREE_ITEM_CHILDREN_COLLECTION_ALIAS } from './tree-item-children/constants.js';
import { manifests as folderManifests } from './folder/manifests.js';
import { manifests as reloadTreeItemChildrenManifest } from './reload-tree-item-children/manifests.js';
import { manifests as treeItemChildrenManifests } from './tree-item-children/manifests.js';
import { UmbStylesheetTreeStore } from './stylesheet-tree.store.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const UMB_STYLESHEET_TREE_ALIAS = 'Umb.Tree.Stylesheet';
export const UMB_STYLESHEET_TREE_REPOSITORY_ALIAS = 'Umb.Repository.StylesheetTree';
/**
 * @deprecated Use {@link UMB_STYLESHEET_TREE_REPOSITORY_ALIAS} instead. This will be removed in Umbraco 18.
 */
export const UMB_STYLESHEET_TREE_STORE_ALIAS = 'Umb.Store.StylesheetTree';

const UMB_STYLESHEET_ROOT_WORKSPACE_ALIAS = 'Umb.Workspace.Stylesheet.Root';

export const manifests: Array<UmbExtensionManifest> = [
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
		type: 'workspaceView',
		kind: 'collection',
		alias: 'Umb.WorkspaceView.Stylesheet.TreeItemChildrenCollection',
		name: 'Stylesheet Tree Item Children Collection Workspace View',
		meta: {
			label: '#general_items',
			pathname: 'items',
			icon: 'icon-grid',
			collectionAlias: UMB_STYLESHEET_TREE_ITEM_CHILDREN_COLLECTION_ALIAS,
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				oneOf: [UMB_STYLESHEET_ROOT_WORKSPACE_ALIAS, 'Umb.Workspace.Stylesheet.Folder'],
			},
		],
	},
	...folderManifests,
	...reloadTreeItemChildrenManifest,
	...treeItemChildrenManifests,
];
