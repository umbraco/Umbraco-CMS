import { UMB_ELEMENT_ENTITY_TYPE, UMB_ELEMENT_FOLDER_ENTITY_TYPE } from '../../entity.js';
import { UMB_ELEMENT_RECYCLE_BIN_ROOT_ENTITY_TYPE } from '../root/constants.js';
import { UMB_ELEMENT_RECYCLE_BIN_TREE_ALIAS, UMB_ELEMENT_RECYCLE_BIN_TREE_REPOSITORY_ALIAS } from './constants.js';
import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';
import type { ManifestTree } from '@umbraco-cms/backoffice/tree';
import type { ManifestTreeItemRecycleBinKind } from '@umbraco-cms/backoffice/recycle-bin';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_ELEMENT_RECYCLE_BIN_TREE_REPOSITORY_ALIAS,
	name: 'Element Recycle Bin Tree Repository',
	api: () => import('./element-recycle-bin-tree.repository.js'),
};

const tree: ManifestTree = {
	type: 'tree',
	kind: 'default',
	alias: UMB_ELEMENT_RECYCLE_BIN_TREE_ALIAS,
	name: 'Element Recycle Bin Tree',
	meta: {
		repositoryAlias: UMB_ELEMENT_RECYCLE_BIN_TREE_REPOSITORY_ALIAS,
	},
};

const treeItem: ManifestTreeItemRecycleBinKind = {
	type: 'treeItem',
	kind: 'recycleBin',
	alias: 'Umb.TreeItem.Element.RecycleBin',
	name: 'Element Recycle Bin Tree Item',
	forEntityTypes: [UMB_ELEMENT_RECYCLE_BIN_ROOT_ENTITY_TYPE],
	meta: {
		supportedEntityTypes: [UMB_ELEMENT_ENTITY_TYPE, UMB_ELEMENT_FOLDER_ENTITY_TYPE],
	},
};

export const manifests: Array<UmbExtensionManifest> = [repository, tree, treeItem];
