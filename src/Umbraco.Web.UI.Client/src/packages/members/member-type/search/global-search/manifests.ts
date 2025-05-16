import { UMB_MEMBER_TYPE_SEARCH_PROVIDER_ALIAS } from '../constants.js';
import { UMB_MEMBER_TYPE_GLOBAL_SEARCH_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		name: 'Member Type Global Search',
		alias: UMB_MEMBER_TYPE_GLOBAL_SEARCH_ALIAS,
		type: 'globalSearch',
		weight: 200,
		meta: {
			label: 'Member Types',
			searchProviderAlias: UMB_MEMBER_TYPE_SEARCH_PROVIDER_ALIAS,
		},
	},
];
