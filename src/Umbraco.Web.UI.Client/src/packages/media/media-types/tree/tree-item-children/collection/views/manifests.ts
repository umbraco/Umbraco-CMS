import { UMB_MEDIA_TYPE_TREE_ITEM_CHILDREN_COLLECTION_ALIAS } from '../constants.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionView',
		alias: 'Umb.CollectionView.MediaType.TreeItem.Table',
		name: 'Media Type Tree Item Table Collection View',
		element: () => import('./media-type-tree-item-table-collection-view.element.js'),
		weight: 300,
		meta: {
			label: 'Table',
			icon: 'icon-list',
			pathName: 'table',
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_MEDIA_TYPE_TREE_ITEM_CHILDREN_COLLECTION_ALIAS,
			},
		],
	},
];
