import { UMB_HELP_MENU_ALIAS } from './constants.js';
import { UMB_CURRENT_USER_IS_ADMIN_CONDITION_ALIAS } from '@umbraco-cms/backoffice/current-user';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'menu',
		alias: UMB_HELP_MENU_ALIAS,
		name: 'Help Menu',
	},
	{
		type: 'menuItem',
		kind: 'link',
		alias: 'Umb.MenuItem.Help.LearningBase',
		name: 'Learning Base Help Menu Item',
		weight: 200,
		meta: {
			menus: [UMB_HELP_MENU_ALIAS],
			label: 'Umbraco Learning Base',
			icon: 'icon-movie-alt',
			href: 'https://umbra.co/ulb',
		},
		conditions: [
			{
				alias: UMB_CURRENT_USER_IS_ADMIN_CONDITION_ALIAS,
			},
		],
	},
	{
		type: 'menuItem',
		kind: 'link',
		alias: 'Umb.MenuItem.Help.CommunityForum',
		name: 'The Community Forumn Help Menu Item',
		weight: 100,
		meta: {
			menus: [UMB_HELP_MENU_ALIAS],
			label: 'Community Forum',
			icon: 'icon-hearts',
			href: 'https://forum.umbraco.com/',
		},
		conditions: [
			{
				alias: UMB_CURRENT_USER_IS_ADMIN_CONDITION_ALIAS,
			},
		],
	},
];
