import {
	UMB_USER_PERMISSION_CONTEXT_UMBRACO_NEWS,
	UMB_USER_PERMISSION_UMBRACO_NEWS_BROWSE,
} from './user-permission/constants.js';
import { manifests as userPermissionManifests } from './user-permission/manifests.js';
import { UMB_CONTEXTUAL_USER_PERMISSION_CONDITION_ALIAS } from '@umbraco-cms/backoffice/user-permission';
import { UMB_CONTENT_SECTION_ALIAS } from '@umbraco-cms/backoffice/content';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'dashboard',
		alias: 'Umb.Dashboard.UmbracoNews',
		name: 'Umbraco News Dashboard',
		element: () => import('./umbraco-news-dashboard.element.js'),
		weight: 20,
		meta: {
			label: '#dashboardTabs_contentIntro',
		},
		conditions: [
			{
				alias: 'Umb.Condition.SectionAlias',
				match: UMB_CONTENT_SECTION_ALIAS,
			},
			{
				alias: UMB_CONTEXTUAL_USER_PERMISSION_CONDITION_ALIAS,
				context: UMB_USER_PERMISSION_CONTEXT_UMBRACO_NEWS,
				allOf: [UMB_USER_PERMISSION_UMBRACO_NEWS_BROWSE],
			},
		],
	},
	...userPermissionManifests,
];
