import { UmbMemberTypeRepository } from '../repository/member-type.repository';
import type { ManifestTree } from '@umbraco-cms/models';

const treeAlias = 'Umb.Tree.MemberTypes';

const tree: ManifestTree = {
	type: 'tree',
	alias: treeAlias,
	name: 'Member Types Tree',
	meta: {
		repository: UmbMemberTypeRepository,
	},
};

export const manifests = [tree];
