import { UMB_MEDIA_TYPE_SEARCH_PROVIDER_ALIAS } from '../constants.js';
import { UMB_MEDIA_TYPE_GLOBAL_SEARCH_ALIAS } from './constants.js';
import { UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS } from '@umbraco-cms/backoffice/section';
import { UMB_SETTINGS_SECTION_ALIAS } from '@umbraco-cms/backoffice/settings';

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
		conditions: [
			{
				alias: UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS,
				match: UMB_SETTINGS_SECTION_ALIAS,
			},
		],
	},
];
