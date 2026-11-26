import { UMB_SEARCH_INDEX_PROVIDER_NAME_CONDITION_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'Search Index Provider Name Condition',
		alias: UMB_SEARCH_INDEX_PROVIDER_NAME_CONDITION_ALIAS,
		api: () => import('./indexProviderName.condition.js'),
	},
];
