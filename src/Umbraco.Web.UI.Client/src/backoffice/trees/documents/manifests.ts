import type { ManifestTree, ManifestTreeItemAction } from '@umbraco-cms/models';

const treeAlias = 'Umb.Tree.Documents';

const tree: ManifestTree = {
	type: 'tree',
	alias: treeAlias,
	name: 'Documents Tree',
	weight: 100,
	meta: {
		label: 'Documents',
		icon: 'umb:folder',
		sections: ['Umb.Section.Content'],
		storeContextAlias: 'umbDocumentStore',
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
			trees: [treeAlias],
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
			trees: [treeAlias],
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
			trees: [treeAlias],
			label: 'Paged',
			icon: 'favorite',
		},
	},
];

export const manifests = [tree, ...treeItemActions];
