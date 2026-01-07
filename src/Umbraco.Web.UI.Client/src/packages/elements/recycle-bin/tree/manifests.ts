import { UMB_ELEMENT_ENTITY_TYPE } from '../../entity.js';
import { UMB_ELEMENT_FOLDER_ENTITY_TYPE } from '../../tree/folder/entity.js';
import { UMB_ELEMENT_RECYCLE_BIN_ROOT_ENTITY_TYPE } from '../root/constants.js';
import { UMB_ELEMENT_RECYCLE_BIN_TREE_ALIAS, UMB_ELEMENT_RECYCLE_BIN_TREE_REPOSITORY_ALIAS } from './constants.js';
import { manifests as dataManifests } from './data/manifests.js';
import type { ManifestTree } from '@umbraco-cms/backoffice/tree';
import type { ManifestTreeItemRecycleBinKind } from '@umbraco-cms/backoffice/recycle-bin';

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

export const manifests: Array<UmbExtensionManifest> = [tree, treeItem, ...dataManifests];
