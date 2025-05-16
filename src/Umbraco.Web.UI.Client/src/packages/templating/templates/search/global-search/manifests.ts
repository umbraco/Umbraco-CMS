import { UMB_TEMPLATE_SEARCH_PROVIDER_ALIAS } from '../constants.js';
import { UMB_TEMPLATE_GLOBAL_SEARCH_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		name: 'Template Global Search',
		alias: UMB_TEMPLATE_GLOBAL_SEARCH_ALIAS,
		type: 'globalSearch',
		weight: 200,
		meta: {
			label: 'Templates',
			searchProviderAlias: UMB_TEMPLATE_SEARCH_PROVIDER_ALIAS,
		},
	},
];
