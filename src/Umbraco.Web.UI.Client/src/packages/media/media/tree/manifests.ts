import { UMB_MEDIA_ENTITY_TYPE, UMB_MEDIA_ROOT_ENTITY_TYPE } from '../entity.js';
import { UmbMediaTreeRepository } from './media-tree.repository.js';
import { UmbMediaTreeStore } from './media-tree.store.js';
import { manifests as reloadTreeItemChildrenManifests } from './reload-tree-item-children/manifests.js';
import type {
	ManifestRepository,
	ManifestTree,
	ManifestTreeItem,
	ManifestTreeStore,
} from '@umbraco-cms/backoffice/extension-registry';

export const UMB_MEDIA_TREE_REPOSITORY_ALIAS = 'Umb.Repository.Media.Tree';
export const UMB_MEDIA_TREE_STORE_ALIAS = 'Umb.Store.Media.Tree';
export const UMB_MEDIA_TREE_ALIAS = 'Umb.Tree.Media';

const treeRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_MEDIA_TREE_REPOSITORY_ALIAS,
	name: 'Media Tree Repository',
	api: UmbMediaTreeRepository,
};

const treeStore: ManifestTreeStore = {
	type: 'treeStore',
	alias: UMB_MEDIA_TREE_STORE_ALIAS,
	name: 'Media Tree Store',
	api: UmbMediaTreeStore,
};

const tree: ManifestTree = {
	type: 'tree',
	alias: UMB_MEDIA_TREE_ALIAS,
	name: 'Media Tree',
	meta: {
		repositoryAlias: UMB_MEDIA_TREE_REPOSITORY_ALIAS,
	},
};

const treeItem: ManifestTreeItem = {
	type: 'treeItem',
	kind: 'entity',
	alias: 'Umb.TreeItem.Media',
	name: 'Media Tree Item',
	meta: {
		entityTypes: [UMB_MEDIA_ROOT_ENTITY_TYPE, UMB_MEDIA_ENTITY_TYPE],
	},
};

export const manifests = [treeRepository, treeStore, tree, treeItem, ...reloadTreeItemChildrenManifests];
