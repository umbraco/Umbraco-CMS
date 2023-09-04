import {
	SCRIPTS_ENTITY_TYPE,
	SCRIPTS_REPOSITORY_ALIAS,
	SCRIPTS_ROOT_ENTITY_TYPE,
	SCRIPTS_TREE_ALIAS,
} from '../config.js';
import type { ManifestTree, ManifestTreeItem } from '@umbraco-cms/backoffice/extension-registry';

const tree: ManifestTree = {
	type: 'tree',
	alias: SCRIPTS_TREE_ALIAS,
	name: 'Scripts Tree',
	weight: 30,
	meta: {
		repositoryAlias: SCRIPTS_REPOSITORY_ALIAS,
	},
};

const treeItem: ManifestTreeItem = {
	type: 'treeItem',
	kind: 'fileSystem',
	alias: 'Umb.TreeItem.Scripts',
	name: 'Scripts Tree Item',
	meta: {
		entityTypes: [SCRIPTS_ROOT_ENTITY_TYPE, SCRIPTS_ENTITY_TYPE],
	},
};

export const manifests = [tree, treeItem];
