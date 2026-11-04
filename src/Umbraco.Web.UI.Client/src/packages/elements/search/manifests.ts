import { UMB_ELEMENT_ENTITY_TYPE } from '../entity.js';
import { UMB_ELEMENT_SEARCH_PROVIDER_ALIAS } from './constants.js';
import { manifests as globalSearchManifests } from './global-search/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		name: 'Element Search Provider',
		alias: UMB_ELEMENT_SEARCH_PROVIDER_ALIAS,
		type: 'searchProvider',
		api: () => import('./element.search-provider.js'),
		weight: 700,
		meta: {
			label: 'Elements',
		},
	},
	{
		name: 'Element Search Result Item',
		alias: 'Umb.SearchResultItem.Element',
		type: 'searchResultItem',
		element: () => import('./element-search-result-item.element.js'),
		forEntityTypes: [UMB_ELEMENT_ENTITY_TYPE],
	},
	...globalSearchManifests,
];
