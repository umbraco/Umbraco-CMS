import {
	UMB_MEDIA_TYPE_ENTITY_TYPE,
	UMB_MEDIA_TYPE_ROOT_ENTITY_TYPE,
	UMB_MEDIA_TYPE_FOLDER_ENTITY_TYPE,
} from '../entity.js';
import {
	UMB_MEDIA_TYPE_TREE_ALIAS,
	UMB_MEDIA_TYPE_TREE_REPOSITORY_ALIAS,
	UMB_MEDIA_TYPE_TREE_STORE_ALIAS,
} from './constants.js';
import { manifests as folderManifests } from './folder/manifests.js';
import { manifests as treeItemChildrenManifest } from './tree-item-children/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MEDIA_TYPE_TREE_REPOSITORY_ALIAS,
		name: 'Media Type Tree Repository',
		api: () => import('./media-type-tree.repository.js'),
	},
	{
		type: 'treeStore',
		alias: UMB_MEDIA_TYPE_TREE_STORE_ALIAS,
		name: 'Media Type Tree Store',
		api: () => import('./media-type-tree.store.js'),
	},
	{
		type: 'tree',
		kind: 'default',
		alias: UMB_MEDIA_TYPE_TREE_ALIAS,
		name: 'Media Type Tree',
		meta: {
			repositoryAlias: UMB_MEDIA_TYPE_TREE_REPOSITORY_ALIAS,
		},
	},
	{
		type: 'treeItem',
		kind: 'default',
		alias: 'Umb.TreeItem.MediaType',
		name: 'Media Type Tree Item',
		forEntityTypes: [UMB_MEDIA_TYPE_ENTITY_TYPE, UMB_MEDIA_TYPE_ROOT_ENTITY_TYPE, UMB_MEDIA_TYPE_FOLDER_ENTITY_TYPE],
	},
	...folderManifests,
	...treeItemChildrenManifest,
];
