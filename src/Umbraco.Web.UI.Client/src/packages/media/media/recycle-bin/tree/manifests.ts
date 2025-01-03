import { UMB_MEDIA_RECYCLE_BIN_ROOT_ENTITY_TYPE } from '../constants.js';
import {
	UMB_MEDIA_RECYCLE_BIN_TREE_ALIAS,
	UMB_MEDIA_RECYCLE_BIN_TREE_REPOSITORY_ALIAS,
	UMB_MEDIA_RECYCLE_BIN_TREE_STORE_ALIAS,
} from './constants.js';
import { manifests as reloadTreeItemChildrenManifests } from './reload-tree-item-children/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MEDIA_RECYCLE_BIN_TREE_REPOSITORY_ALIAS,
		name: 'Media Recycle Bin Tree Repository',
		api: () => import('./media-recycle-bin-tree.repository.js'),
	},
	{
		type: 'treeStore',
		alias: UMB_MEDIA_RECYCLE_BIN_TREE_STORE_ALIAS,
		name: 'Media Recycle Bin Tree Store',
		api: () => import('./media-recycle-bin-tree.store.js'),
	},
	{
		type: 'tree',
		kind: 'default',
		alias: UMB_MEDIA_RECYCLE_BIN_TREE_ALIAS,
		name: 'Media Recycle Bin Tree',
		meta: {
			repositoryAlias: UMB_MEDIA_RECYCLE_BIN_TREE_REPOSITORY_ALIAS,
		},
	},
	{
		type: 'treeItem',
		kind: 'default',
		alias: 'Umb.TreeItem.Media.RecycleBin',
		name: 'Media Recycle Bin Tree Item',
		forEntityTypes: [UMB_MEDIA_RECYCLE_BIN_ROOT_ENTITY_TYPE],
	},
	{
		type: 'workspace',
		kind: 'default',
		alias: 'Umb.Workspace.Media.RecycleBin.Root',
		name: 'Media Recycle Bin Root Workspace',
		meta: {
			entityType: UMB_MEDIA_RECYCLE_BIN_ROOT_ENTITY_TYPE,
			headline: '#general_recycleBin',
		},
	},
	...reloadTreeItemChildrenManifests,
];
