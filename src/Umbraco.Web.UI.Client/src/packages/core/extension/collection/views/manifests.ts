import { UMB_COLLECTION_CONDITION_ALIAS } from '@umbraco-cms/backoffice/collection';
import UmbExtensionTableCollectionViewElement from './table/extension-table-collection-view.element.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionView',
		alias: 'Umb.CollectionView.Extension.Table',
		name: 'Extension Table Collection View',
		element: UmbExtensionTableCollectionViewElement,
		meta: {
			label: 'Table',
			icon: 'icon-table',
			pathName: 'table',
		},
		conditions: [
			{
				alias: UMB_COLLECTION_CONDITION_ALIAS,
				match: 'Umb.Collection.Extension',
			},
		],
	},
];
