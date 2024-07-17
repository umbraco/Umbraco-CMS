import { manifests as welcomeDashboardManifests } from './welcome-dashboard/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_SETTINGS_SECTION_ALIAS = 'Umb.Section.Settings';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'section',
		alias: UMB_SETTINGS_SECTION_ALIAS,
		name: 'Settings Section',
		weight: 800,
		meta: {
			label: '#sections_settings',
			pathname: 'settings',
		},
		conditions: [
			{
				alias: 'Umb.Condition.SectionUserPermission',
				match: UMB_SETTINGS_SECTION_ALIAS,
			},
		],
	},
	{
		type: 'menu',
		alias: 'Umb.Menu.StructureSettings',
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
			menu: 'Umb.Menu.StructureSettings',
		},
		conditions: [
			{
				alias: 'Umb.Condition.SectionAlias',
				match: UMB_SETTINGS_SECTION_ALIAS,
			},
		],
	},
	{
		type: 'menu',
		alias: 'Umb.Menu.AdvancedSettings',
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
			menu: 'Umb.Menu.AdvancedSettings',
		},
		conditions: [
			{
				alias: 'Umb.Condition.SectionAlias',
				match: UMB_SETTINGS_SECTION_ALIAS,
			},
		],
	},
	...welcomeDashboardManifests,
];
