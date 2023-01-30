import { UmbTemplateTreeRepository } from './template.tree.repository';
import type { ManifestTree, ManifestTreeItemAction } from '@umbraco-cms/models';

const tree: ManifestTree = {
	type: 'tree',
	alias: 'Umb.Tree.Templates',
	name: 'Templates Tree',
	meta: {
		repository: UmbTemplateTreeRepository,
	},
};

const treeItemActions: Array<ManifestTreeItemAction> = [
	{
		type: 'treeItemAction',
		alias: 'Umb.TreeItemAction.Template.Create',
		name: 'Create Template Tree Action',
		loader: () => import('./actions/create/create-template-tree-action.element'),
		weight: 200,
		meta: {
			entityType: 'template',
			label: 'Create',
			icon: 'umb:add',
		},
	},
];

export const manifests = [tree, ...treeItemActions];
