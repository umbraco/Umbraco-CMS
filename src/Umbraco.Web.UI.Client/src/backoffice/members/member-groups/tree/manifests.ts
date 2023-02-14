import { UmbMemberGroupRepository } from '../repository/member-group.repository';
import type { ManifestTree } from '@umbraco-cms/models';

const treeAlias = 'Umb.Tree.MemberGroups';

const tree: ManifestTree = {
	type: 'tree',
	alias: treeAlias,
	name: 'Member Groups Tree',
	weight: 100,
	meta: {
		repository: UmbMemberGroupRepository,
	},
};

export const manifests = [tree];
