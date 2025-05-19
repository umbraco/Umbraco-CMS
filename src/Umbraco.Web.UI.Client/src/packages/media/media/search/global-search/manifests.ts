import { UMB_MEDIA_SECTION_ALIAS } from '../../../media-section/constants.js';
import { UMB_MEDIA_SEARCH_PROVIDER_ALIAS } from '../constants.js';
import { UMB_MEDIA_GLOBAL_SEARCH_ALIAS } from './constants.js';
import { UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS } from '@umbraco-cms/backoffice/section';

export const manifests: Array<UmbExtensionManifest> = [
	{
		name: 'Media Global Search',
		alias: UMB_MEDIA_GLOBAL_SEARCH_ALIAS,
		type: 'globalSearch',
		weight: 700,
		api: () => import('./media-global-search.js'),
		meta: {
			label: 'Media',
			searchProviderAlias: UMB_MEDIA_SEARCH_PROVIDER_ALIAS,
		},
		conditions: [
			{
				alias: UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS,
				match: UMB_MEDIA_SECTION_ALIAS,
			},
		],
	},
];
