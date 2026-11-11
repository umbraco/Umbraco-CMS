import { UmbDefaultFacetFilterActiveValueApi } from './default-facet-filter-active-value.api.js';
import { UmbDefaultFacetFilterActiveValueElement } from './default-facet-filter-active-value.element.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.FacetFilterActiveValue.Default',
		matchKind: 'default',
		matchType: 'facetFilterActiveValue',
		manifest: {
			type: 'facetFilterActiveValue',
			kind: 'default',
			element: UmbDefaultFacetFilterActiveValueElement,
			api: UmbDefaultFacetFilterActiveValueApi,
		},
	},
];
