import { UMB_MEDIA_TYPE_SEARCH_PROVIDER_ALIAS } from '../constants.js';
import { UMB_MEDIA_TYPE_GLOBAL_SEARCH_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		name: 'Media Type Global Search',
		alias: UMB_MEDIA_TYPE_GLOBAL_SEARCH_ALIAS,
		type: 'globalSearch',
		weight: 500,
		meta: {
			label: 'Media Types',
			searchProviderAlias: UMB_MEDIA_TYPE_SEARCH_PROVIDER_ALIAS,
		},
	},
];
