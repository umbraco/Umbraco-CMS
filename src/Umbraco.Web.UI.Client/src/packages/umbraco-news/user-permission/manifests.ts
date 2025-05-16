import { UMB_UMBRACO_NEWS_UI_USER_PERMISSION, UMB_USER_PERMISSION_UMBRACO_NEWS_SEE } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'uiUserPermission',
		alias: 'Umb.UiUserPermission.UmbracoNews.Dashboard.See',
		name: 'See Umbraco News Dashboard User Permission',
		meta: {
			label: 'See',
			description: 'Allow to see the Getting Started Dashboard',
			group: 'Getting Started',
			permission: {
				context: UMB_UMBRACO_NEWS_UI_USER_PERMISSION,
				verbs: [UMB_USER_PERMISSION_UMBRACO_NEWS_SEE],
			},
		},
	},
];
