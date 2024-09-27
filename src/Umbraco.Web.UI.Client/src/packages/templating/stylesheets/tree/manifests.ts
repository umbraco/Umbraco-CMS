import {
	UMB_STYLESHEET_ENTITY_TYPE,
	UMB_STYLESHEET_FOLDER_ENTITY_TYPE,
	UMB_STYLESHEET_ROOT_ENTITY_TYPE,
} from '../entity.js';
import { manifests as folderManifests } from './folder/manifests.js';
import { manifests as reloadTreeItemChildrenManifest } from './reload-tree-item-children/manifests.js';

export const UMB_STYLESHEET_TREE_ALIAS = 'Umb.Tree.Stylesheet';
export const UMB_STYLESHEET_TREE_REPOSITORY_ALIAS = 'Umb.Repository.StylesheetTree';
export const UMB_STYLESHEET_TREE_STORE_ALIAS = 'Umb.Store.StylesheetTree';

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
		api: () => import('./stylesheet-tree.store.js'),
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
		alias: 'Umb.Workspace.Stylesheet.Root',
		name: 'Stylesheet Root Workspace',
		meta: {
			entityType: UMB_STYLESHEET_ROOT_ENTITY_TYPE,
			headline: '#treeHeaders_stylesheets',
		},
	},
	...folderManifests,
	...reloadTreeItemChildrenManifest,
];
