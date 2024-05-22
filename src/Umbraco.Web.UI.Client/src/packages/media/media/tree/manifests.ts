import { UMB_MEDIA_ENTITY_TYPE, UMB_MEDIA_ROOT_ENTITY_TYPE } from '../entity.js';
import { manifests as reloadTreeItemChildrenManifests } from './reload-tree-item-children/manifests.js';
import type {
	ManifestRepository,
	ManifestTree,
	ManifestTreeItem,
	ManifestTreeStore,
	ManifestTypes,
} from '@umbraco-cms/backoffice/extension-registry';

export const UMB_MEDIA_TREE_REPOSITORY_ALIAS = 'Umb.Repository.Media.Tree';
export const UMB_MEDIA_TREE_STORE_ALIAS = 'Umb.Store.Media.Tree';
export const UMB_MEDIA_TREE_ALIAS = 'Umb.Tree.Media';

const treeRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_MEDIA_TREE_REPOSITORY_ALIAS,
	name: 'Media Tree Repository',
	api: () => import('./media-tree.repository.js'),
};

const treeStore: ManifestTreeStore = {
	type: 'treeStore',
	alias: UMB_MEDIA_TREE_STORE_ALIAS,
	name: 'Media Tree Store',
	api: () => import('./media-tree.store.js'),
};

const tree: ManifestTree = {
	type: 'tree',
	alias: UMB_MEDIA_TREE_ALIAS,
	name: 'Media Tree',
	element: () => import('./media-tree.element.js'),
	api: () => import('./media-tree.context.js'),
	meta: {
		repositoryAlias: UMB_MEDIA_TREE_REPOSITORY_ALIAS,
	},
};

const treeItem: ManifestTreeItem = {
	type: 'treeItem',
	kind: 'default',
	alias: 'Umb.TreeItem.Media',
	name: 'Media Tree Item',
	element: () => import('./tree-item/media-tree-item.element.js'),
	api: () => import('./tree-item/media-tree-item.context.js'),
	forEntityTypes: [UMB_MEDIA_ENTITY_TYPE],
};

const rootTreeItem: ManifestTreeItem = {
	type: 'treeItem',
	kind: 'default',
	alias: 'Umb.TreeItem.Media.Root',
	name: 'Media Tree Root',
	forEntityTypes: [UMB_MEDIA_ROOT_ENTITY_TYPE],
};

export const manifests: Array<ManifestTypes> = [
	treeRepository,
	treeStore,
	tree,
	treeItem,
	rootTreeItem,
	...reloadTreeItemChildrenManifests,
];
