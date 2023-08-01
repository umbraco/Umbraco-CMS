import { MEMBER_TYPES_REPOSITORY_ALIAS } from '../repository/manifests.js';
import type { ManifestTree, ManifestTreeItem } from '@umbraco-cms/backoffice/extension-registry';

const treeAlias = 'Umb.Tree.MemberTypes';

const tree: ManifestTree = {
	type: 'tree',
	alias: treeAlias,
	name: 'Member Types Tree',
	meta: {
		repositoryAlias: MEMBER_TYPES_REPOSITORY_ALIAS,
	},
};

const treeItem: ManifestTreeItem = {
	type: 'treeItem',
	kind: 'entity',
	alias: 'Umb.TreeItem.MemberType',
	name: 'Member Type Tree Item',
	meta: {
		entityTypes: ['member-type-root', 'member-type'],
	},
};

export const manifests = [tree, treeItem];
