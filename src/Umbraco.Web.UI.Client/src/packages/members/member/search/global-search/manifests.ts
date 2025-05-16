import { UMB_MEMBER_SEARCH_PROVIDER_ALIAS } from '../constants.js';
import { UMB_MEMBER_GLOBAL_SEARCH_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		name: 'Member Global Search',
		alias: UMB_MEMBER_GLOBAL_SEARCH_ALIAS,
		type: 'globalSearch',
		weight: 300,
		meta: {
			label: 'Members',
			searchProviderAlias: UMB_MEMBER_SEARCH_PROVIDER_ALIAS,
		},
	},
];
