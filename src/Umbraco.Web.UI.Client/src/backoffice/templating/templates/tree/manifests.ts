import { UmbTemplateRepository } from '../repository/template.repository';
import type { ManifestTree, ManifestTreeItemAction } from '@umbraco-cms/models';

const tree: ManifestTree = {
	type: 'tree',
	alias: 'Umb.Tree.Templates',
	name: 'Templates Tree',
	meta: {
		repository: UmbTemplateRepository,
	},
};

const treeItemActions: Array<ManifestTreeItemAction> = [
	{
		type: 'treeItemAction',
		alias: 'Umb.TreeItemAction.Template.Create',
		name: 'Create Template Tree Action',
		loader: () => import('./actions/create/create-template-tree-action.element'),
		weight: 300,
		meta: {
			entityType: 'template',
			label: 'Create',
			icon: 'umb:add',
		},
	},
	{
		type: 'treeItemAction',
		alias: 'Umb.TreeItemAction.Template.Delete',
		name: 'Delete Template Tree Action',
		loader: () => import('./actions/delete/delete-template-tree-action.element'),
		weight: 200,
		meta: {
			entityType: 'template',
			label: 'Delete',
			icon: 'umb:trash',
		},
	},
];

export const manifests = [tree, ...treeItemActions];
