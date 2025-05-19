import { UMB_DOCUMENT_SEARCH_PROVIDER_ALIAS } from '../constants.js';
import { UMB_DOCUMENT_GLOBAL_SEARCH_ALIAS } from './constants.js';
import { UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS } from '@umbraco-cms/backoffice/section';
import { UMB_CONTENT_SECTION_ALIAS } from '@umbraco-cms/backoffice/content';

export const manifests: Array<UmbExtensionManifest> = [
	{
		name: 'Document Global Search',
		alias: UMB_DOCUMENT_GLOBAL_SEARCH_ALIAS,
		type: 'globalSearch',
		weight: 800,
		api: () => import('./document-global-search.js'),
		meta: {
			label: 'Documents',
			searchProviderAlias: UMB_DOCUMENT_SEARCH_PROVIDER_ALIAS,
		},
		conditions: [
			{
				alias: UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS,
				match: UMB_CONTENT_SECTION_ALIAS,
			},
		],
	},
];
