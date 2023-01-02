import type { ManifestTree, ManifestTreeItemAction, ManifestWorkspace } from '@umbraco-cms/extensions-registry';

const alias = 'DataType';
const treeAlias = `Umb.Tree.${alias}`;
const workspaceAlias = `Umb.Workspace.${alias}`;

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
		loader: () => import('./tree/actions/create/action-data-type-create.element'),
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
		loader: () => import('./tree/actions/delete/action-data-type-delete.element'),
		weight: 100,
		meta: {
			trees: [treeAlias],
			label: 'Delete',
			icon: 'umb:delete',
		},
	},
];

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: workspaceAlias,
	name: 'Data Type Workspace',
	loader: () => import('./workspace/workspace-data-type.element'),
	meta: {
		entityType: 'data-type',
	},
};

export const manifests = [tree, ...treeItemActions, workspace];
