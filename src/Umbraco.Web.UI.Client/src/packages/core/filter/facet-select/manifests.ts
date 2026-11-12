import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.Filter.FacetSelect',
		matchKind: 'facetSelect',
		matchType: 'filter',
		manifest: {
			type: 'filter',
			kind: 'facetSelect',
			element: () => import('../select/select-filter.element.js'),
			api: () => import('./facet-select-filter.api.js'),
		},
	},
];
