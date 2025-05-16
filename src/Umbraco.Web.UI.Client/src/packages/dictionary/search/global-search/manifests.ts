import { UMB_DICTIONARY_SEARCH_PROVIDER_ALIAS } from '../constants.js';
import { UMB_DICTIONARY_GLOBAL_SEARCH_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		name: 'Dictionary Global Search',
		alias: UMB_DICTIONARY_GLOBAL_SEARCH_ALIAS,
		type: 'globalSearch',
		weight: 600,
		meta: {
			label: 'Dictionary',
			searchProviderAlias: UMB_DICTIONARY_SEARCH_PROVIDER_ALIAS,
		},
	},
];
