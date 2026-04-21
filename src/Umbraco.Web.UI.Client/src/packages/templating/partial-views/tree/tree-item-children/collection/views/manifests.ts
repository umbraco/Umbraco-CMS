import { UMB_PARTIAL_VIEW_TREE_ITEM_CHILDREN_COLLECTION_ALIAS } from '../constants.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionView',
		alias: 'Umb.CollectionView.PartialView.TreeItem.Table',
		name: 'Partial View Tree Item Table Collection View',
		element: () => import('./partial-view-tree-item-table-collection-view.element.js'),
		weight: 300,
		meta: {
			label: 'Table',
			icon: 'icon-table',
			pathName: 'table',
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_PARTIAL_VIEW_TREE_ITEM_CHILDREN_COLLECTION_ALIAS,
			},
		],
	},
];
