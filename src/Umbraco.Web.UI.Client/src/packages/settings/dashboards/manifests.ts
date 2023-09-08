import type { ManifestDashboard } from '@umbraco-cms/backoffice/extension-registry';

const sectionAlias = 'Umb.Section.Settings';

const dashboards: Array<ManifestDashboard> = [
	{
		type: 'dashboard',
		alias: 'Umb.Dashboard.SettingsWelcome',
		name: 'Welcome Settings Dashboard',
		elementName: 'umb-dashboard-settings-welcome',
		loader: () => import('./settings-welcome/dashboard-settings-welcome.element.js'),
		weight: 500,
		meta: {
			label: 'Welcome',
			pathname: 'welcome',
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
		alias: 'Umb.Dashboard.ModelsBuilder',
		name: 'Models Builder Dashboard',
		elementName: 'umb-dashboard-models-builder',
		loader: () => import('./models-builder/dashboard-models-builder.element.js'),
		weight: 300,
		meta: {
			label: 'Models Builder',
			pathname: 'models-builder',
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
		alias: 'Umb.Dashboard.PublishedStatus',
		name: 'Published Status Dashboard',
		elementName: 'umb-dashboard-published-status',
		loader: () => import('./published-status/dashboard-published-status.element.js'),
		weight: 200,
		meta: {
			label: 'Published Status',
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
		elementName: 'umb-dashboard-performance-profiling',
		loader: () => import('./performance-profiling/dashboard-performance-profiling.element.js'),
		weight: 101,
		meta: {
			label: 'Profiling',
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
		elementName: 'umb-dashboard-telemetry',
		loader: () => import('./telemetry/dashboard-telemetry.element.js'),
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

export const manifests = [...dashboards];
