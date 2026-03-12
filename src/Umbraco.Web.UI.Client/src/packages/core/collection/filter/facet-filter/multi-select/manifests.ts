import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.CollectionFacetFilter.MultiSelect',
		matchKind: 'multiSelect',
		matchType: 'collectionFacetFilter',
		manifest: {
			type: 'collectionFacetFilter',
			kind: 'multiSelect',
			element: () => import('./default-multi-select-collection-facet-filter.element.js'),
			api: () => import('./default-multi-select-collection-facet-filter.api.js'),
		},
	},
];
