import { UMB_SETTINGS_SECTION_ALIAS } from '../section/index.js';
import type { ManifestTypes, UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes | UmbBackofficeManifestKind> = [
	{
		type: 'dashboard',
		alias: 'Umb.Dashboard.SettingsWelcome',
		name: 'Welcome Settings Dashboard',
		element: () => import('./settings-welcome-dashboard.element.js'),
		weight: 500,
		meta: {
			label: '#dashboardTabs_settingsWelcome',
			pathname: 'welcome',
		},
		conditions: [
			{
				alias: 'Umb.Condition.SectionAlias',
				match: UMB_SETTINGS_SECTION_ALIAS,
			},
		],
	},
];
