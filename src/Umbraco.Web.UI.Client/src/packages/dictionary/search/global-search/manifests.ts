import { UMB_DICTIONARY_SEARCH_PROVIDER_ALIAS } from '../constants.js';
import { UMB_DICTIONARY_GLOBAL_SEARCH_ALIAS } from './constants.js';
import { UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS } from '@umbraco-cms/backoffice/section';
import { UMB_TRANSLATION_SECTION_ALIAS } from '@umbraco-cms/backoffice/translation';

export const manifests: Array<UmbExtensionManifest> = [
	{
		name: 'Dictionary Global Search',
		alias: UMB_DICTIONARY_GLOBAL_SEARCH_ALIAS,
		type: 'globalSearch',
		weight: 600,
		meta: {
			label: 'Dictionary',
			searchProviderAlias: UMB_DICTIONARY_SEARCH_PROVIDER_ALIAS,
		},
		conditions: [
			{
				alias: UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS,
				match: UMB_TRANSLATION_SECTION_ALIAS,
			},
		],
	},
];
