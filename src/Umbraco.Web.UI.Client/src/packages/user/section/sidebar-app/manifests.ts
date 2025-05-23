import { UMB_USER_MANAGEMENT_SECTION_ALIAS } from '../constants.js';
import { UMB_USER_MANAGEMENT_MENU_ALIAS } from '../menu/index.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'sectionSidebarApp',
		kind: 'menu',
		alias: 'Umb.SectionSidebarApp.Menu.UserManagement',
		name: 'User Management Menu Sidebar App',
		weight: 100,
		meta: {
			label: '#treeHeaders_users',
			menu: UMB_USER_MANAGEMENT_MENU_ALIAS,
		},
		conditions: [
			{
				alias: 'Umb.Condition.SectionAlias',
				match: UMB_USER_MANAGEMENT_SECTION_ALIAS,
			},
		],
	},
];
