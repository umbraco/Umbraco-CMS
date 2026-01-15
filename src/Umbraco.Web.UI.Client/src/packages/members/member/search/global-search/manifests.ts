import { UMB_MEMBER_MANAGEMENT_SECTION_ALIAS } from '../../../section/constants.js';
import { UMB_MEMBER_SEARCH_PROVIDER_ALIAS } from '../constants.js';
import { UMB_MEMBER_GLOBAL_SEARCH_ALIAS } from './constants.js';
import { UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS } from '@umbraco-cms/backoffice/section';

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
		conditions: [
			{
				alias: UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS,
				match: UMB_MEMBER_MANAGEMENT_SECTION_ALIAS,
			},
		],
	},
];
