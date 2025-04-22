import { UMB_DICTIONARY_ENTITY_TYPE } from '../entity.js';
import { UMB_DICTIONARY_SEARCH_PROVIDER_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		name: 'Dictionary Search Provider',
		alias: UMB_DICTIONARY_SEARCH_PROVIDER_ALIAS,
		type: 'searchProvider',
		api: () => import('./dictionary.search-provider.js'),
		weight: 600,
		meta: {
			label: 'Dictionary',
		},
	},
	{
		name: 'Dictionary Search Result Item',
		alias: 'Umb.SearchResultItem.Dictionary',
		type: 'searchResultItem',
		forEntityTypes: [UMB_DICTIONARY_ENTITY_TYPE],
	},
];
