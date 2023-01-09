import type { ManifestTree, ManifestTreeItemAction } from '@umbraco-cms/models';

const tree: ManifestTree = {
	type: 'tree',
	alias: 'Umb.Tree.DataTypes',
	name: 'Data Types Tree',
	weight: 100,
	meta: {
		storeAlias: 'umbDataTypeStore',
	},
};

const treeItemActions: Array<ManifestTreeItemAction> = [
	{
		type: 'treeItemAction',
		alias: 'Umb.TreeItemAction.DataType.Create',
		name: 'Tree Item Action Create',
		loader: () => import('./actions/create/action-data-type-create.element'),
		weight: 200,
		meta: {
			entityType: 'data-type',
			label: 'Create',
			icon: 'umb:add',
		},
	},
	{
		type: 'treeItemAction',
		alias: 'Umb.TreeItemAction.DataType.Delete',
		name: 'Tree Item Action Delete',
		loader: () => import('./actions/delete/action-data-type-delete.element'),
		weight: 100,
		meta: {
			entityType: 'data-type',
			label: 'Delete',
			icon: 'umb:delete',
		},
	},
];

export const manifests = [tree, ...treeItemActions];
