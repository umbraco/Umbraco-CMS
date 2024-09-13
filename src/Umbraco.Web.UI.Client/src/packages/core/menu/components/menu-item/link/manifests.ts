import type { ManifestTypes, UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes | UmbBackofficeManifestKind> = [
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
