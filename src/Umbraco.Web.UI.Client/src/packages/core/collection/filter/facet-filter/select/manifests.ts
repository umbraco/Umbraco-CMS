import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.CollectionFacetFilter.Select',
		matchKind: 'select',
		matchType: 'collectionFacetFilter',
		manifest: {
			type: 'collectionFacetFilter',
			kind: 'select',
			element: () => import('./default-select-collection-facet-filter.element.js'),
			api: () => import('./default-select-collection-facet-filter.api.js'),
		},
	},
];
