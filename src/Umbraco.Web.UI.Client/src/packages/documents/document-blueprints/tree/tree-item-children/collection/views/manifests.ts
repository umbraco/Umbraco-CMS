import { UMB_DOCUMENT_BLUEPRINT_TREE_ITEM_CHILDREN_COLLECTION_ALIAS } from '../constants.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionView',
		alias: 'Umb.CollectionView.DocumentBlueprint.TreeItem.Table',
		name: 'Document Blueprint Tree Item Table Collection View',
		element: () => import('./document-blueprint-tree-item-table-collection-view.element.js'),
		weight: 300,
		meta: {
			label: 'Table',
			icon: 'icon-table',
			pathName: 'table',
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_DOCUMENT_BLUEPRINT_TREE_ITEM_CHILDREN_COLLECTION_ALIAS,
			},
		],
	},
];
