import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Example.Kind.FacetFilter.Range',
		matchKind: 'range',
		matchType: 'facetFilter',
		manifest: {
			type: 'facetFilter',
			kind: 'range',
			element: () => import('./range-facet-filter.element.js'),
			api: () => import('./range-facet-filter.api.js'),
		},
	},
];
