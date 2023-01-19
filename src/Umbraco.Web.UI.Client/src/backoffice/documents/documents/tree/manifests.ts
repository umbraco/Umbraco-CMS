import { STORE_ALIAS } from '../document.store';
import type { ManifestTree, ManifestTreeItemAction } from '@umbraco-cms/models';

const treeAlias = 'Umb.Tree.Documents';

const tree: ManifestTree = {
	type: 'tree',
	alias: treeAlias,
	name: 'Documents Tree',
	meta: {
		storeAlias: STORE_ALIAS,
	},
};

const treeItemActions: Array<ManifestTreeItemAction> = [
	{
		type: 'treeItemAction',
		alias: 'Umb.TreeItemAction.Document.Create',
		name: 'Document Tree Item Action Create',
		loader: () => import('./actions/action-document-create.element'),
		weight: 100,
		meta: {
			entityType: 'document',
			label: 'Create',
			icon: 'add',
		},
	},
	{
		type: 'treeItemAction',
		alias: 'Umb.TreeItemAction.Document.Delete',
		name: 'Document Tree Item Action Delete',
		loader: () => import('./actions/action-document-delete.element'),
		weight: 100,
		meta: {
			entityType: 'document',
			label: 'Delete',
			icon: 'delete',
		},
	},
	{
		type: 'treeItemAction',
		alias: 'Umb.TreeItemAction.Document.Paged',
		name: 'Document Tree Item Action Paged',
		loader: () => import('./actions/action-document-paged.element'),
		weight: 100,
		meta: {
			entityType: 'document',
			label: 'Paged',
			icon: 'favorite',
		},
	},
];

export const manifests = [tree, ...treeItemActions];
