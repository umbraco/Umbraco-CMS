import { UMB_MEMBER_MANAGEMENT_SECTION_ALIAS } from '../constants.js';
import { UMB_MEMBER_MANAGEMENT_MENU_ALIAS } from '../menu/index.js';
import { UMB_SECTION_ALIAS_CONDITION_ALIAS } from '@umbraco-cms/backoffice/section';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'sectionSidebarApp',
		kind: 'menu',
		alias: 'Umb.SectionSidebarApp.Menu.MemberManagement',
		name: 'Member Management Menu Sidebar App',
		weight: 100,
		meta: {
			label: '#treeHeaders_member',
			menu: UMB_MEMBER_MANAGEMENT_MENU_ALIAS,
		},
		conditions: [
			{
				alias: UMB_SECTION_ALIAS_CONDITION_ALIAS,
				match: UMB_MEMBER_MANAGEMENT_SECTION_ALIAS,
			},
		],
	},
];
