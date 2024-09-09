import { UMB_HELP_MENU_ALIAS } from './constants.js';
import type { ManifestTypes, UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes | UmbBackofficeManifestKind> = [
	{
		type: 'menu',
		alias: UMB_HELP_MENU_ALIAS,
		name: 'Help Menu',
	},
	{
		type: 'menuItem',
		alias: 'Umb.MenuItem.Help.LearningBase',
		name: 'Learning Base Help Menu Item',
		weight: 200,
		meta: {
			label: 'Umbraco Learning Base',
			icon: 'icon-movie-alt',
			menus: [UMB_HELP_MENU_ALIAS],
		},
	},
	{
		type: 'menuItem',
		alias: 'Umb.MenuItem.Help.CommunityWebsite',
		name: 'Community Website Help Menu Item',
		weight: 100,
		meta: {
			label: 'Community Website',
			icon: 'icon-hearts',
			menus: [UMB_HELP_MENU_ALIAS],
		},
	},
];
