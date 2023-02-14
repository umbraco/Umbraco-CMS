import { UmbTemplateRepository } from '../repository/template.repository';
import type { ManifestTree } from '@umbraco-cms/models';

const tree: ManifestTree = {
	type: 'tree',
	alias: 'Umb.Tree.Templates',
	name: 'Templates Tree',
	meta: {
		repository: UmbTemplateRepository,
	},
};

export const manifests = [tree];
