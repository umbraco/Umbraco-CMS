import type { ManifestTypes, UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes | UmbBackofficeManifestKind> = [
	{
		type: 'headerApp',
		alias: 'Umb.HeaderApp.Help',
		name: 'Help Header App',
		element: () => import('./help-header-app.element.js'),
		weight: 0,
		meta: {
			label: 'TODO: how should we enable this to not be set.',
			icon: 'TODO: how should we enable this to not be set.',
			pathname: 'help',
		},
	},
];
