import type { ManifestDashboard, ManifestSection } from '@umbraco-cms/models';

const sectionAlias = 'Umb.Section.Settings';

const section: ManifestSection = {
	type: 'section',
	alias: sectionAlias,
	name: 'Settings Section',
	weight: 300,
	meta: {
		label: 'Settings',
		pathname: 'settings',
	},
};

const dashboards: Array<ManifestDashboard> = [
	{
		type: 'dashboard',
		alias: 'Umb.Dashboard.SettingsWelcome',
		name: 'Welcome Settings Dashboard',
		elementName: 'umb-dashboard-settings-welcome',
		loader: () => import('../../../dashboards/settings-welcome/dashboard-settings-welcome.element'),
		weight: 500,
		meta: {
			label: 'Welcome',
			sections: [sectionAlias],
			pathname: 'welcome',
		},
	},
	{
		type: 'dashboard',
		alias: 'Umb.Dashboard.ExamineManagement',
		name: 'Examine Management Dashboard',
		elementName: 'umb-dashboard-examine-management',
		loader: () => import('../../../dashboards/examine-management/dashboard-examine-management.element'),
		weight: 400,
		meta: {
			label: 'Examine Management',
			sections: [sectionAlias],
			pathname: 'examine-management',
		},
	},
	{
		type: 'dashboard',
		alias: 'Umb.Dashboard.ModelsBuilder',
		name: 'Models Builder Dashboard',
		elementName: 'umb-dashboard-models-builder',
		loader: () => import('../../../dashboards/models-builder/dashboard-models-builder.element'),
		weight: 300,
		meta: {
			label: 'Models Builder',
			sections: [sectionAlias],
			pathname: 'models-builder',
		},
	},
	{
		type: 'dashboard',
		alias: 'Umb.Dashboard.PublishedStatus',
		name: 'Published Status Dashboard',
		elementName: 'umb-dashboard-published-status',
		loader: () => import('../../../dashboards/published-status/dashboard-published-status.element'),
		weight: 200,
		meta: {
			label: 'Published Status',
			sections: [sectionAlias],
			pathname: 'published-status',
		},
	},
	{
		type: 'dashboard',
		alias: 'Umb.Dashboard.Profiling',
		name: 'Profiling',
		elementName: 'umb-dashboard-performance-profiling',
		loader: () => import('../../../dashboards/performance-profiling/dashboard-performance-profiling.element'),
		weight: 101,
		meta: {
			label: 'Profiling',
			sections: [sectionAlias],
			pathname: 'profiling',
		},
	},
	{
		type: 'dashboard',
		alias: 'Umb.Dashboard.Telemetry',
		name: 'Telemetry',
		elementName: 'umb-dashboard-telemetry',
		loader: () => import('../../../dashboards/telemetry/dashboard-telemetry.element'),
		weight: 100,
		meta: {
			label: 'Telemetry Data',
			sections: [sectionAlias],
			pathname: 'telemetry',
		},
	},
];

export const manifests = [section, ...dashboards];
