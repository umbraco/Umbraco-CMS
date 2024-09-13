import { UMB_USER_MANAGEMENT_SECTION_ALIAS } from '../constants.js';
import { UMB_USER_MANAGEMENT_MENU_ALIAS } from '../menu/index.js';
import type { ManifestTypes, UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes | UmbExtensionManifestKind> = [
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
