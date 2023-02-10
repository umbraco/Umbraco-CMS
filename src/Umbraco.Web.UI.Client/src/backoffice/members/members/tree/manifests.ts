import { UmbMemberRepository } from '../repository/member.repository';
import type { ManifestTree } from '@umbraco-cms/models';

const tree: ManifestTree = {
	type: 'tree',
	alias: 'Umb.Tree.Members',
	name: 'Members Tree',
	weight: 10,
	meta: {
		repository: UmbMemberRepository,
	},
};

export const manifests = [tree];
