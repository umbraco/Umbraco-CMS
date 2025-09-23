import { UMB_PROPERTY_EDITOR_DATA_SOURCE_SEARCH_PROVIDER_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'searchProvider',
		alias: UMB_PROPERTY_EDITOR_DATA_SOURCE_SEARCH_PROVIDER_ALIAS,
		name: 'Property Editor Data Source Search Provider',
		api: () => import('./search-provider.js'),
		weight: 600,
	},
];
