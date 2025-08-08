import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'dashboardApp',
		alias: 'Umb.DashboardApp.DocumentationIntro',
		name: 'Documentation Intro Dashboard App',
		weight: 300,
		element: () => import('./documentation-intro-dashboard-app.element.js'),
		meta: {
			headline: '#settingsDashboard_documentationHeader',
			size: 'small',
		},
	},
	{
		type: 'dashboardApp',
		alias: 'Umb.DashboardApp.VideosIntro',
		name: 'Videos Intro Dashboard App',
		weight: 400,
		element: () => import('./videos-intro-dashboard-app.element.js'),
		meta: {
			headline: '#settingsDashboard_videosHeader',
			size: 'large',
		},
	},
];
