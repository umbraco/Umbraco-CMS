import { EXAMPLE_DYNAMIC_FACET_COLLECTION_ALIAS } from '../collection/constants.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionView',
		alias: 'Example.DynamicFacetFilter.CollectionView.Table',
		name: 'Dynamic Facet Filter Table Collection View',
		js: () => import('./collection-view.element.js'),
		weight: 100,
		meta: {
			label: 'Table',
			icon: 'icon-table',
			pathName: 'table',
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: EXAMPLE_DYNAMIC_FACET_COLLECTION_ALIAS,
			},
		],
	},
];
