import { UMB_ELEMENT_ENTITY_TYPE, UMB_ELEMENT_ROOT_ENTITY_TYPE } from '../entity.js';
import { UMB_ELEMENT_TREE_ALIAS, UMB_ELEMENT_TREE_REPOSITORY_ALIAS } from './constants.js';
import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';
import type { ManifestTree, ManifestTreeItem } from '@umbraco-cms/backoffice/tree';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_ELEMENT_TREE_REPOSITORY_ALIAS,
	name: 'Element Tree Repository',
	api: () => import('./element-tree.repository.js'),
};

const tree: ManifestTree = {
	type: 'tree',
	kind: 'default',
	alias: UMB_ELEMENT_TREE_ALIAS,
	name: 'Element Tree',
	meta: {
		repositoryAlias: UMB_ELEMENT_TREE_REPOSITORY_ALIAS,
	},
};

const treeItems: Array<ManifestTreeItem> = [
	{
		type: 'treeItem',
		kind: 'default',
		alias: 'Umb.TreeItem.Element',
		name: 'Element Tree Item',
		api: () => import('./element-tree-item.context.js'),
		forEntityTypes: [UMB_ELEMENT_ENTITY_TYPE],
	},
	{
		type: 'treeItem',
		kind: 'default',
		alias: 'Umb.TreeItem.Element.Root',
		name: 'Element Tree Root',
		forEntityTypes: [UMB_ELEMENT_ROOT_ENTITY_TYPE],
	},
];

export const manifests: Array<UmbExtensionManifest> = [repository, tree, ...treeItems];
