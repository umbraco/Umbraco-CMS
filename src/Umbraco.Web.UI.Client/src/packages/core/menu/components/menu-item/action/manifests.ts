import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.MenuItem.Action',
		matchKind: 'action',
		matchType: 'menuItem',
		manifest: {
			type: 'menuItem',
			kind: 'action',
			api: () => import('./action-menu-item.api.js'),
			element: () => import('./action-menu-item.element.js'),
		},
	},
];
