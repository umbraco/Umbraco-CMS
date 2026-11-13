import { UmbFacetSelectFilterApi, UmbSelectFilterApi, UmbSelectFilterElement } from '@umbraco-cms/backoffice/filter';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.CollectionFilter.Select',
		matchKind: 'select',
		matchType: 'collectionFilter',
		manifest: {
			type: 'collectionFilter',
			kind: 'select',
			element: UmbSelectFilterElement,
			api: UmbSelectFilterApi,
		},
	},
	{
		type: 'kind',
		alias: 'Umb.Kind.CollectionFilter.FacetSelect',
		matchKind: 'facetSelect',
		matchType: 'collectionFilter',
		manifest: {
			type: 'collectionFilter',
			kind: 'facetSelect',
			element: UmbSelectFilterElement,
			api: UmbFacetSelectFilterApi,
		},
	},
];
