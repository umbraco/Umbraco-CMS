import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.CollectionFilter.Select',
		matchKind: 'select',
		matchType: 'collectionFilter',
		manifest: {
			type: 'collectionFilter',
			kind: 'select',
			element: () => import('./default-select-collection-filter.element.js'),
			api: () => import('./default-select-collection-filter.api.js'),
		},
	},
];
