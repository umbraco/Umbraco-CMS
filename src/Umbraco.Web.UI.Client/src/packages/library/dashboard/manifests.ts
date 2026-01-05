import { UMB_LIBRARY_SECTION_ALIAS } from '../section/index.js';
import { UMB_LIBRARY_DASHBOARD_ALIAS } from './constants.js';
import { UMB_SECTION_ALIAS_CONDITION_ALIAS } from '@umbraco-cms/backoffice/section';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'dashboard',
		alias: UMB_LIBRARY_DASHBOARD_ALIAS,
		name: 'Library Welcome Dashboard',
		element: () => import('./library-welcome-dashboard.element.js'),
		weight: 500,
		meta: {
			label: '#dashboardTabs_libraryWelcome',
			pathname: 'welcome',
		},
		conditions: [
			{
				alias: UMB_SECTION_ALIAS_CONDITION_ALIAS,
				match: UMB_LIBRARY_SECTION_ALIAS,
			},
		],
	},
];
