import type { ManifestTypes, UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes | UmbBackofficeManifestKind> = [
	{
		type: 'headerApp',
		alias: 'Umb.HeaderApp.Help',
		name: 'Help Header App',
		element: () => import('./help-header-app.element.js'),
		weight: 500,
	},
];
