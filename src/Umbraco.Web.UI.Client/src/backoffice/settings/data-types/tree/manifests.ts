import { UmbDataTypeRepository } from '../repository/data-type.repository';
import type { ManifestTree, ManifestTreeItem } from '@umbraco-cms/backoffice/extensions-registry';

const tree: ManifestTree = {
	type: 'tree',
	alias: 'Umb.Tree.DataTypes',
	name: 'Data Types Tree',
	meta: {
		repository: UmbDataTypeRepository,
	},
};

const treeItem: ManifestTreeItem = {
	type: 'treeItem',
	kind: 'entity',
	alias: 'Umb.TreeItem.DataType',
	name: 'Data Type Tree Item',
	conditions: {
		entityType: 'data-type',
	},
};

export const manifests = [tree, treeItem];
