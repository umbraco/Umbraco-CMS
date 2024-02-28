import {
	UMB_MEDIA_TYPE_ENTITY_TYPE,
	UMB_MEDIA_TYPE_ROOT_ENTITY_TYPE,
	UMB_MEDIA_TYPE_FOLDER_ENTITY_TYPE,
} from '../entity.js';
import { manifests as folderManifests } from './folder/manifests.js';
import { manifests as reloadTreeItemChildrenManifest } from './reload-tree-item-children/manifests.js';
import type {
	ManifestRepository,
	ManifestTree,
	ManifestTreeItem,
	ManifestTreeStore,
} from '@umbraco-cms/backoffice/extension-registry';

export const UMB_MEDIA_TYPE_TREE_REPOSITORY_ALIAS = 'Umb.Repository.MediaType.Tree';
export const UMB_MEDIA_TYPE_TREE_STORE_ALIAS = 'Umb.Store.MediaType.Tree';
export const UMB_MEDIA_TYPE_TREE_ALIAS = 'Umb.Tree.MediaType';

const treeRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_MEDIA_TYPE_TREE_REPOSITORY_ALIAS,
	name: 'Media Type Tree Repository',
	api: () => import('./media-type-tree.repository.js'),
};

const treeStore: ManifestTreeStore = {
	type: 'treeStore',
	alias: UMB_MEDIA_TYPE_TREE_STORE_ALIAS,
	name: 'Media Type Tree Store',
	api: () => import('./media-type-tree.store.js'),
};

const tree: ManifestTree = {
	type: 'tree',
	kind: 'default',
	alias: UMB_MEDIA_TYPE_TREE_ALIAS,
	name: 'Media Type Tree',
	meta: {
		repositoryAlias: UMB_MEDIA_TYPE_TREE_REPOSITORY_ALIAS,
	},
};

const treeItem: ManifestTreeItem = {
	type: 'treeItem',
	kind: 'default',
	alias: 'Umb.TreeItem.MediaType',
	name: 'Media Type Tree Item',
	meta: {
		entityTypes: [UMB_MEDIA_TYPE_ENTITY_TYPE, UMB_MEDIA_TYPE_ROOT_ENTITY_TYPE, UMB_MEDIA_TYPE_FOLDER_ENTITY_TYPE],
	},
};

export const manifests = [
	treeRepository,
	treeStore,
	tree,
	treeItem,
	...folderManifests,
	...reloadTreeItemChildrenManifest,
];
