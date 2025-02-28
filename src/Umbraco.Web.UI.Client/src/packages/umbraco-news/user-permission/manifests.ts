import { UMB_USER_PERMISSION_CONTEXT_UMBRACO_NEWS, UMB_USER_PERMISSION_UMBRACO_NEWS_BROWSE } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'contextualUserPermission',
		alias: 'Umb.UserPermission.UmbracoNews.Dashboard.Browse',
		name: 'Browse Umbraco News Dashboard User Permission',
		meta: {
			label: 'Browse',
			description: 'Allow access to browse the Umbraco News Dashboard',
			group: 'Umbraco News',
			permission: {
				context: UMB_USER_PERMISSION_CONTEXT_UMBRACO_NEWS,
				verbs: [UMB_USER_PERMISSION_UMBRACO_NEWS_BROWSE],
			},
		},
	},
];
