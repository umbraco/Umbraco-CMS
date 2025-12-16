import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.CollectionTextFilter.Default',
		matchKind: 'default',
		matchType: 'collectionTextFilter',
		manifest: {
			type: 'collectionTextFilter',
			kind: 'default',
			element: () => import('./default-collection-text-filter.element.js'),
			api: () => import('./default-collection-text-filter.api.js'),
		},
	},
];
