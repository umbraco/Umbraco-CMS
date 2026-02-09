import { UMB_MEDIA_ENTITY_TYPE } from '../../entity.js';
import { UMB_MEDIA_RECYCLE_BIN_ROOT_ENTITY_TYPE } from '../root/entity.js';
import {
	UMB_MEDIA_RECYCLE_BIN_TREE_ALIAS,
	UMB_MEDIA_RECYCLE_BIN_TREE_REPOSITORY_ALIAS,
	UMB_MEDIA_RECYCLE_BIN_TREE_STORE_ALIAS,
} from './constants.js';
import { UmbMediaRecycleBinTreeStore } from './media-recycle-bin-tree.store.js';
import { manifests as reloadTreeItemChildrenManifests } from './reload-tree-item-children/manifests.js';
import { manifests as treeItemChildrenManifests } from './tree-item-children/manifests.js';

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
		api: UmbMediaRecycleBinTreeStore,
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
		kind: 'recycleBin',
		alias: 'Umb.TreeItem.Media.RecycleBin',
		name: 'Media Recycle Bin Tree Item',
		forEntityTypes: [UMB_MEDIA_RECYCLE_BIN_ROOT_ENTITY_TYPE],
		meta: {
			supportedEntityTypes: [UMB_MEDIA_ENTITY_TYPE],
		},
	},
	...reloadTreeItemChildrenManifests,
	...treeItemChildrenManifests,
];
