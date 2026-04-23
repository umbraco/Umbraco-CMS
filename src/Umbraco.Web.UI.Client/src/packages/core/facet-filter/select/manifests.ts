import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.FacetFilter.Select',
		matchKind: 'select',
		matchType: 'facetFilter',
		manifest: {
			type: 'facetFilter',
			kind: 'select',
			element: () => import('./select-facet-filter.element.js'),
			api: () => import('./select-facet-filter.api.js'),
		},
	},
];
