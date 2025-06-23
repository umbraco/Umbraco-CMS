import { EXAMPLE_COLLECTION_ALIAS } from '../constants.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionView',
		alias: 'Example.CollectionView.Card',
		name: 'Example Card Collection View',
		js: () => import('./collection-view.element.js'),
		weight: 50,
		meta: {
			label: 'Card',
			icon: 'icon-grid',
			pathName: 'card',
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: EXAMPLE_COLLECTION_ALIAS,
			},
		],
	},
];
