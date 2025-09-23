import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.CollectionMenu.Default',
		matchKind: 'default',
		matchType: 'collectionMenu',
		manifest: {
			type: 'collectionMenu',
			kind: 'default',
			element: () => import('./default-collection-menu.element.js'),
			api: () => import('./default-collection-menu.context.js'),
		},
	},
];
