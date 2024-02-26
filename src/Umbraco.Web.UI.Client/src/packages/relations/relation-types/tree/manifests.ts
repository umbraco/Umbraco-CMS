import { UMB_RELATION_TYPE_ENTITY_TYPE, UMB_RELATION_TYPE_ROOT_ENTITY_TYPE } from '../entity.js';
import { manifests as reloadTreeItemChildrenManifest } from './reload-tree-item-children/manifests.js';
import type {
	ManifestRepository,
	ManifestTree,
	ManifestTreeItem,
	ManifestTreeStore,
} from '@umbraco-cms/backoffice/extension-registry';

export const UMB_RELATION_TYPE_TREE_REPOSITORY_ALIAS = 'Umb.Repository.RelationType.Tree';
export const UMB_RELATION_TYPE_TREE_STORE_ALIAS = 'Umb.Store.RelationType.Tree';
export const UMB_RELATION_TYPE_TREE_ALIAS = 'Umb.Tree.RelationType';

const treeRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_RELATION_TYPE_TREE_REPOSITORY_ALIAS,
	name: 'Relation Type Tree Repository',
	api: () => import('./relation-type-tree.repository.js'),
};

const treeStore: ManifestTreeStore = {
	type: 'treeStore',
	alias: UMB_RELATION_TYPE_TREE_STORE_ALIAS,
	name: 'Relation Type Tree Store',
	api: () => import('./relation-type-tree.store.js'),
};

const tree: ManifestTree = {
	type: 'tree',
	alias: UMB_RELATION_TYPE_TREE_ALIAS,
	name: 'Relation Type Tree',
	meta: {
		repositoryAlias: UMB_RELATION_TYPE_TREE_REPOSITORY_ALIAS,
	},
};

const treeItem: ManifestTreeItem = {
	type: 'treeItem',
	kind: 'default',
	alias: 'Umb.TreeItem.RelationType',
	name: 'RelationType Tree Item',
	meta: {
		entityTypes: [UMB_RELATION_TYPE_ROOT_ENTITY_TYPE, UMB_RELATION_TYPE_ENTITY_TYPE],
	},
};

export const manifests = [treeRepository, treeStore, tree, treeItem, ...reloadTreeItemChildrenManifest];
