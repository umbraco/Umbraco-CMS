import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'headerApp',
		alias: 'Umb.HeaderApp.Search',
		name: 'Header App Search',
		loader: () => import('./umb-search-header-app.element.js'),
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
		loader: () => import('./search-modal/search-modal.element.js'),
	},
	{
		type: 'dashboard',
		alias: 'Umb.Dashboard.ExamineManagement',
		name: 'Examine Management Dashboard',
		elementName: 'umb-dashboard-examine-management',
		loader: () => import('./examine-management-dashboard/dashboard-examine-management.element.js'),
		weight: 400,
		meta: {
			label: 'Examine Management',
			pathname: 'examine-management',
		},
		conditions: [
			{
				alias: 'Umb.Condition.SectionAlias',
				match: 'Umb.Section.Settings',
			},
		],
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.ExamineFieldsSettings',
		name: 'Examine Field Settings Modal',
		loader: () => import('./examine-management-dashboard/views/modal-views/fields-settings.element.js'),
	},
];
