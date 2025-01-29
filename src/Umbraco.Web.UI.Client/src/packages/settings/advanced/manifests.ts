import { UMB_SETTINGS_SECTION_ALIAS } from '../constants.js';
import { UMB_ADVANCED_SETTINGS_MENU_ALIAS } from './constants.js';

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
				alias: 'Umb.Condition.SectionAlias',
				match: UMB_SETTINGS_SECTION_ALIAS,
			},
		],
	},
];
