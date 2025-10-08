import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.Dashboard.Default',
		matchKind: 'default',
		matchType: 'dashboard',
		manifest: {
			type: 'dashboard',
			element: () => import('./dashboard.element.js'),
			api: () => import('./dashboard.context.js'),
		},
	},
];
