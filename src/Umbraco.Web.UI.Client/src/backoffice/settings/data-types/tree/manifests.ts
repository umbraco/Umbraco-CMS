import { UmbDataTypeRepository } from '../repository/data-type.repository';
import type { ManifestTree } from '@umbraco-cms/backoffice/extensions-registry';

const tree: ManifestTree = {
	type: 'tree',
	alias: 'Umb.Tree.DataTypes',
	name: 'Data Types Tree',
	meta: {
		repository: UmbDataTypeRepository,
	},
};

export const manifests = [tree];
