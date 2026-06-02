import { UMB_ELEMENT_SEARCH_PROVIDER_ALIAS } from '../constants.js';
import { UMB_ELEMENT_GLOBAL_SEARCH_ALIAS } from './constants.js';
import { UMB_LIBRARY_SECTION_ALIAS } from '@umbraco-cms/backoffice/library';
import { UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS } from '@umbraco-cms/backoffice/section';

export const manifests: Array<UmbExtensionManifest> = [
	{
		name: 'Element Global Search',
		alias: UMB_ELEMENT_GLOBAL_SEARCH_ALIAS,
		type: 'globalSearch',
		weight: 700,
		meta: {
			label: 'Elements',
			searchProviderAlias: UMB_ELEMENT_SEARCH_PROVIDER_ALIAS,
			sectionAlias: UMB_LIBRARY_SECTION_ALIAS,
		},
		conditions: [
			{
				alias: UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS,
				match: UMB_LIBRARY_SECTION_ALIAS,
			},
		],
	},
];
