import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.MenuItem.Link',
		matchKind: 'link',
		matchType: 'menuItem',
		manifest: {
			type: 'menuItem',
			element: () => import('./link-menu-item.element.js'),
		},
	},
];
