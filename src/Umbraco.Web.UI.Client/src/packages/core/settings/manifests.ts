import { manifests as welcomeDashboardManifests } from './welcome-dashboard/manifests.js';

export const UMB_SETTINGS_SECTION_ALIAS = 'Umb.Section.Settings';

export const manifests = [
	{
		type: 'section',
		alias: UMB_SETTINGS_SECTION_ALIAS,
		name: 'Settings Section',
		weight: 400,
		meta: {
			label: 'Settings',
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
		alias: 'Umb.Menu.Settings',
		name: 'Settings Menu',
		meta: {
			label: 'Settings',
		},
	},
	{
		type: 'sectionSidebarApp',
		kind: 'menu',
		alias: 'Umb.SectionSidebarMenu.Settings',
		name: 'Settings Section Sidebar Menu',
		weight: 200,
		meta: {
			label: 'Settings',
			menu: 'Umb.Menu.Settings',
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
