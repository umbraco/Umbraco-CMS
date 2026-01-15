import { UMB_DATA_TYPE_SEARCH_PROVIDER_ALIAS } from '../constants.js';
import { UMB_DATA_TYPE_GLOBAL_SEARCH_ALIAS } from './constants.js';
import { UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS } from '@umbraco-cms/backoffice/section';
import { UMB_SETTINGS_SECTION_ALIAS } from '@umbraco-cms/backoffice/settings';

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
		conditions: [
			{
				alias: UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS,
				match: UMB_SETTINGS_SECTION_ALIAS,
			},
		],
	},
];
