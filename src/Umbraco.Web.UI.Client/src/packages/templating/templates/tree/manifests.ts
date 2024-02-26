import { UMB_TEMPLATE_ENTITY_TYPE, UMB_TEMPLATE_ROOT_ENTITY_TYPE } from '../entity.js';
import { manifests as reloadTreeItemChildrenManifest } from './reload-tree-item-children/manifests.js';
import type {
	ManifestRepository,
	ManifestTree,
	ManifestTreeItem,
	ManifestTreeStore,
} from '@umbraco-cms/backoffice/extension-registry';

export const UMB_TEMPLATE_TREE_REPOSITORY_ALIAS = 'Umb.Repository.Template.Tree';
export const UMB_TEMPLATE_TREE_STORE_ALIAS = 'Umb.Store.Template.Tree';
export const UMB_TEMPLATE_TREE_ALIAS = 'Umb.Tree.Template';

const treeRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_TEMPLATE_TREE_REPOSITORY_ALIAS,
	name: 'Template Tree Repository',
	api: () => import('./template-tree.repository.js'),
};

const treeStore: ManifestTreeStore = {
	type: 'treeStore',
	alias: UMB_TEMPLATE_TREE_STORE_ALIAS,
	name: 'Template Tree Store',
	api: () => import('./template-tree.store.js'),
};

const tree: ManifestTree = {
	type: 'tree',
	alias: UMB_TEMPLATE_TREE_ALIAS,
	name: 'Template Tree',
	meta: {
		repositoryAlias: UMB_TEMPLATE_TREE_REPOSITORY_ALIAS,
	},
};

const treeItem: ManifestTreeItem = {
	type: 'treeItem',
	kind: 'default',
	alias: 'Umb.TreeItem.Template',
	name: 'Template Tree Item',
	meta: {
		entityTypes: [UMB_TEMPLATE_ROOT_ENTITY_TYPE, UMB_TEMPLATE_ENTITY_TYPE],
	},
};

export const manifests = [treeRepository, treeStore, tree, treeItem, ...reloadTreeItemChildrenManifest];
