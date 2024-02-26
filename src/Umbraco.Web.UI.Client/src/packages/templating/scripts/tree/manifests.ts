import { UMB_SCRIPT_ENTITY_TYPE, UMB_SCRIPT_FOLDER_ENTITY_TYPE, UMB_SCRIPT_ROOT_ENTITY_TYPE } from '../entity.js';
import { manifests as folderManifests } from './folder/manifests.js';
import { manifests as reloadTreeItemChildrenManifest } from './reload-tree-item-children/manifests.js';
import type {
	ManifestRepository,
	ManifestTree,
	ManifestTreeItem,
	ManifestTreeStore,
} from '@umbraco-cms/backoffice/extension-registry';

export const UMB_SCRIPT_TREE_REPOSITORY_ALIAS = 'Umb.Repository.Script.Tree';
export const UMB_SCRIPT_TREE_STORE_ALIAS = 'Umb.Store.Script.Tree';
export const UMB_SCRIPT_TREE_ALIAS = 'Umb.Tree.Script';

const treeRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_SCRIPT_TREE_REPOSITORY_ALIAS,
	name: 'Script Tree Repository',
	api: () => import('./script-tree.repository.js'),
};

const treeStore: ManifestTreeStore = {
	type: 'treeStore',
	alias: UMB_SCRIPT_TREE_STORE_ALIAS,
	name: 'Script Tree Store',
	api: () => import('./script-tree.store.js'),
};

const tree: ManifestTree = {
	type: 'tree',
	alias: UMB_SCRIPT_TREE_ALIAS,
	name: 'Script Tree',
	meta: {
		repositoryAlias: UMB_SCRIPT_TREE_REPOSITORY_ALIAS,
	},
};

const treeItem: ManifestTreeItem = {
	type: 'treeItem',
	kind: 'unique',
	alias: 'Umb.TreeItem.Script',
	name: 'Script Tree Item',
	meta: {
		entityTypes: [UMB_SCRIPT_ROOT_ENTITY_TYPE, UMB_SCRIPT_ENTITY_TYPE, UMB_SCRIPT_FOLDER_ENTITY_TYPE],
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
