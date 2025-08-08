import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'dashboardApp',
		alias: 'Umb.DashboardApp.CertificationIntro',
		name: 'Certification Intro Dashboard App',
		weight: 100,
		element: () => import('./certification-intro-dashboard-app.element.js'),
		meta: {
			headline: '#settingsDashboard_trainingHeader',
			size: 'small',
		},
	},
];
