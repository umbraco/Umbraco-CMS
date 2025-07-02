import { UMB_SETTINGS_SECTION_ALIAS } from '../constants.js';
import { UMB_ADVANCED_SETTINGS_MENU_ALIAS } from './constants.js';
import { UMB_SECTION_ALIAS_CONDITION_ALIAS } from '@umbraco-cms/backoffice/section';

export const manifests = [
	{
		type: 'menu',
		alias: UMB_ADVANCED_SETTINGS_MENU_ALIAS,
		name: 'Advanced Settings Menu',
	},
	{
		type: 'sectionSidebarApp',
		kind: 'menu',
		alias: 'Umb.SectionSidebarMenu.AdvancedSettings',
		name: 'Advanced Settings Sidebar Menu',
		weight: 100,
		meta: {
			label: '#treeHeaders_advancedGroup',
			menu: UMB_ADVANCED_SETTINGS_MENU_ALIAS,
		},
		conditions: [
			{
				alias: UMB_SECTION_ALIAS_CONDITION_ALIAS,
				match: UMB_SETTINGS_SECTION_ALIAS,
			},
		],
	},
];
