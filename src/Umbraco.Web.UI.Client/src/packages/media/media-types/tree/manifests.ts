import {
	UMB_MEDIA_TYPE_ENTITY_TYPE,
	UMB_MEDIA_TYPE_ROOT_ENTITY_TYPE,
	UMB_MEDIA_TYPE_FOLDER_ENTITY_TYPE,
} from '../entity.js';
import { UmbMediaTypeTreeRepository } from './media-type-tree.repository.js';
import { UmbMediaTypeTreeStore } from './media-type-tree.store.js';
import { manifests as folderManifests } from './folder/manifests.js';

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
	api: UmbMediaTypeTreeRepository,
};

const treeStore: ManifestTreeStore = {
	type: 'treeStore',
	alias: UMB_MEDIA_TYPE_TREE_STORE_ALIAS,
	name: 'Media Type Tree Store',
	api: UmbMediaTypeTreeStore,
};

const tree: ManifestTree = {
	type: 'tree',
	alias: UMB_MEDIA_TYPE_TREE_ALIAS,
	name: 'Media Type Tree',
	meta: {
		repositoryAlias: UMB_MEDIA_TYPE_TREE_REPOSITORY_ALIAS,
	},
};

const treeItem: ManifestTreeItem = {
	type: 'treeItem',
	kind: 'unique',
	alias: 'Umb.TreeItem.MediaType',
	name: 'Media Type Tree Item',
	meta: {
		entityTypes: [UMB_MEDIA_TYPE_ENTITY_TYPE, UMB_MEDIA_TYPE_ROOT_ENTITY_TYPE, UMB_MEDIA_TYPE_FOLDER_ENTITY_TYPE],
	},
};

export const manifests = [treeRepository, treeStore, tree, treeItem, ...folderManifests];
