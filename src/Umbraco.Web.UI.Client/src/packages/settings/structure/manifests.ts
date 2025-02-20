import { UMB_SETTINGS_SECTION_ALIAS } from '../constants.js';
import { UMB_STRUCTURE_SETTINGS_MENU_ALIAS } from './constants.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'menu',
		alias: UMB_STRUCTURE_SETTINGS_MENU_ALIAS,
		name: 'Settings Menu',
	},
	{
		type: 'sectionSidebarApp',
		kind: 'menu',
		alias: 'Umb.SectionSidebarMenu.Settings',
		name: 'Structure Settings Sidebar Menu',
		weight: 300,
		meta: {
			label: '#treeHeaders_structureGroup',
			menu: UMB_STRUCTURE_SETTINGS_MENU_ALIAS,
		},
		conditions: [
			{
				alias: 'Umb.Condition.SectionAlias',
				match: UMB_SETTINGS_SECTION_ALIAS,
			},
		],
	},
];
