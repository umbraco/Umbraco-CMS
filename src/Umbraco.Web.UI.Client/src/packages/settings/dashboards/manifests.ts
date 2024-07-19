import type { ManifestDashboard, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const sectionAlias = 'Umb.Section.Settings';

const dashboards: Array<ManifestDashboard> = [
	{
		type: 'dashboard',
		alias: 'Umb.Dashboard.PublishedStatus',
		name: 'Published Status Dashboard',
		element: () => import('./published-status/dashboard-published-status.element.js'),
		weight: 300,
		meta: {
			label: '#dashboardTabs_settingsPublishedStatus',
			pathname: 'published-status',
		},
		conditions: [
			{
				alias: 'Umb.Condition.SectionAlias',
				match: sectionAlias,
			},
		],
	},
	{
		type: 'dashboard',
		alias: 'Umb.Dashboard.Profiling',
		name: 'Profiling',
		element: () => import('./performance-profiling/dashboard-performance-profiling.element.js'),
		weight: 101,
		meta: {
			label: '#dashboardTabs_settingsProfiler',
			pathname: 'profiling',
		},
		conditions: [
			{
				alias: 'Umb.Condition.SectionAlias',
				match: sectionAlias,
			},
		],
	},
	{
		type: 'dashboard',
		alias: 'Umb.Dashboard.Telemetry',
		name: 'Telemetry',
		element: () => import('./telemetry/dashboard-telemetry.element.js'),
		weight: 100,
		meta: {
			label: 'Telemetry Data',
			pathname: 'telemetry',
		},
		conditions: [
			{
				alias: 'Umb.Condition.SectionAlias',
				match: sectionAlias,
			},
		],
	},
];

export const manifests: Array<ManifestTypes> = [...dashboards];
