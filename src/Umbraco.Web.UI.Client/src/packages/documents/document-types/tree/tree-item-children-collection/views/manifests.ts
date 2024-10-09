import { UMB_DOCUMENT_TYPE_TREE_ITEM_CHILDREN_COLLECTION_ALIAS } from '../constants.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionView',
		alias: 'Umb.CollectionView.DocumentType.Folder.Table',
		name: 'Document Type Folder Children Table Collection View',
		element: () => import('./table.element.js'),
		weight: 300,
		meta: {
			label: 'Table',
			icon: 'icon-list',
			pathName: 'table',
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_DOCUMENT_TYPE_TREE_ITEM_CHILDREN_COLLECTION_ALIAS,
			},
		],
	},
];
