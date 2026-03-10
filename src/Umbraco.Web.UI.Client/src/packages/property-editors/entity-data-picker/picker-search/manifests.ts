import { UMB_ENTITY_DATA_PICKER_SEARCH_PROVIDER_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'searchProvider',
		alias: UMB_ENTITY_DATA_PICKER_SEARCH_PROVIDER_ALIAS,
		name: 'Entity Data Picker Search Provider',
		api: () => import('./entity-data-picker.search-provider.js'),
	},
];
