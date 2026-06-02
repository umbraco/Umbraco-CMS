import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';
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
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: 'Umb.Collection.Extension',
			},
		],
	},
];
