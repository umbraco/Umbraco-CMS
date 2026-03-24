import { EXAMPLE_DYNAMIC_FACET_COLLECTION_ALIAS } from '../collection/constants.js';
import { manifests as categoryManifests } from './category/manifests.js';
import { manifests as colorManifests } from './color/manifests.js';
import { manifests as priceManifests } from './price/manifests.js';
import { manifests as sizeManifests } from './size/manifests.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionTextFilter',
		kind: 'default',
		alias: 'Example.DynamicFacetFilter.TextFilter',
		name: 'Dynamic Facet Filter Text Filter',
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: EXAMPLE_DYNAMIC_FACET_COLLECTION_ALIAS,
			},
		],
	},
	...categoryManifests,
	...colorManifests,
	...priceManifests,
	...sizeManifests,
];
