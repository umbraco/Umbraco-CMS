import { UmbMemberTypeRepository } from '../repository/member-type.repository';
import type { ManifestTree, ManifestTreeItemAction } from '@umbraco-cms/models';

const treeAlias = 'Umb.Tree.MemberTypes';

const tree: ManifestTree = {
	type: 'tree',
	alias: treeAlias,
	name: 'Member Types Tree',
	meta: {
		repository: UmbMemberTypeRepository
	},
};

const treeItemActions: Array<ManifestTreeItemAction> = [];

export const manifests = [tree, ...treeItemActions];
