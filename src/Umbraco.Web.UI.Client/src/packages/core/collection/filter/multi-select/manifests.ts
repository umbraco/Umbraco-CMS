import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.CollectionFilter.MultiSelect',
		matchKind: 'multiSelect',
		matchType: 'collectionFilter',
		manifest: {
			type: 'collectionFilter',
			kind: 'multiSelect',
			element: () => import('./default-multi-select-collection-filter.element.js'),
			api: () => import('./default-multi-select-collection-filter.api.js'),
		},
	},
];
