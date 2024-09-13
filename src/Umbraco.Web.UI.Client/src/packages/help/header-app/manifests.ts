import type { ManifestTypes, UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes | UmbExtensionManifestKind> = [
	{
		type: 'headerApp',
		alias: 'Umb.HeaderApp.Help',
		name: 'Help Header App',
		element: () => import('./help-header-app.element.js'),
		weight: 500,
	},
];
