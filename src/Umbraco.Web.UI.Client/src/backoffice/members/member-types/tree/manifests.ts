import { UmbMemberTypeRepository } from '../repository/member-type.repository';
import type { ManifestTree, ManifestTreeItem } from '@umbraco-cms/backoffice/extensions-registry';

const treeAlias = 'Umb.Tree.MemberTypes';

const tree: ManifestTree = {
	type: 'tree',
	alias: treeAlias,
	name: 'Member Types Tree',
	meta: {
		repository: UmbMemberTypeRepository,
	},
};

const treeItem: ManifestTreeItem = {
	type: 'treeItem',
	kind: 'entity',
	alias: 'Umb.TreeItem.MemberType',
	name: 'Member Type Tree Item',
	conditions: {
		entityType: 'member-type',
	},
};

export const manifests = [tree, treeItem];
