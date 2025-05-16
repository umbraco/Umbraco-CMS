import { UMB_DATA_TYPE_SEARCH_PROVIDER_ALIAS } from '../constants.js';
import { UMB_DATA_TYPE_GLOBAL_SEARCH_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		name: 'Data Type Global Search',
		alias: UMB_DATA_TYPE_GLOBAL_SEARCH_ALIAS,
		type: 'globalSearch',
		weight: 400,
		meta: {
			label: 'Data Types',
			searchProviderAlias: UMB_DATA_TYPE_SEARCH_PROVIDER_ALIAS,
		},
	},
];
