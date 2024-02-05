import { UMB_SETTINGS_SECTION_ALIAS } from '../manifests.js';

export const manifests = [
	{
		type: 'dashboard',
		alias: 'Umb.Dashboard.SettingsWelcome',
		name: 'Welcome Settings Dashboard',
		element: () => import('./settings-welcome-dashboard.element.js'),
		weight: 500,
		meta: {
			label: 'Welcome',
			pathname: 'welcome',
		},
		conditions: [
			{
				alias: 'Umb.Condition.SectionAlias',
				match: 'Umb.Section.Settings',
			},
		],
	},
];
