import { manifests as examineManifests } from './examine-management-dashboard/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

import './examine-management-dashboard/index.js';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'headerApp',
		alias: 'Umb.HeaderApp.Search',
		name: 'Header App Search',
		js: () => import('./umb-search-header-app.element.js'),
		weight: 900,
		meta: {
			label: 'Search',
			icon: 'search',
			pathname: 'search',
		},
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.Search',
		name: 'Search Modal',
		js: () => import('./search-modal/search-modal.element.js'),
	},
	{
		type: 'dashboard',
		alias: 'Umb.Dashboard.ExamineManagement',
		name: 'Examine Management Dashboard',
		elementName: 'umb-dashboard-examine-management',
		js: () => import('./examine-management-dashboard/dashboard-examine-management.element.js'),
		weight: 400,
		meta: {
			label: '#dashboardTabs_settingsExamine',
			pathname: 'examine-management',
		},
		conditions: [
			{
				alias: 'Umb.Condition.SectionAlias',
				match: 'Umb.Section.Settings',
			},
		],
	},
	...examineManifests,
];
