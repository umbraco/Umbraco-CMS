import { UMB_MEDIA_SEARCH_PROVIDER_ALIAS } from '../constants.js';
import { UMB_MEDIA_GLOBAL_SEARCH_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		name: 'Media Global Search',
		alias: UMB_MEDIA_GLOBAL_SEARCH_ALIAS,
		type: 'globalSearch',
		weight: 700,
		meta: {
			label: 'Media',
			searchProviderAlias: UMB_MEDIA_SEARCH_PROVIDER_ALIAS,
		},
	},
];
