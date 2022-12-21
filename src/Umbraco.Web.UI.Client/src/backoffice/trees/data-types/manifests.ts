import type { ManifestTree, ManifestTreeItemAction } from '@umbraco-cms/models';

const treeAlias = 'Umb.Tree.DataTypes';

const tree: ManifestTree = {
	type: 'tree',
	alias: treeAlias,
	name: 'Data Types Tree',
	weight: 100,
	meta: {
		label: 'Data Types',
		icon: 'umb:folder',
		sections: ['Umb.Section.Settings'],
		storeContextAlias: 'umbDataTypeStore',
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
			trees: [treeAlias],
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
			trees: [treeAlias],
			label: 'Delete',
			icon: 'umb:delete',
		},
	},
];

export const manifests = [tree, ...treeItemActions];
