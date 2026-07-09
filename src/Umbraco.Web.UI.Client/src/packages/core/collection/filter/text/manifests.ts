import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { UmbDefaultCollectionTextFilterElement } from './default-collection-text-filter.element.js';
import { UmbDefaultCollectionTextFilterApi } from './default-collection-text-filter.api.js';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.CollectionTextFilter.Default',
		matchKind: 'default',
		matchType: 'collectionTextFilter',
		manifest: {
			type: 'collectionTextFilter',
			kind: 'default',
			element: UmbDefaultCollectionTextFilterElement,
			api: UmbDefaultCollectionTextFilterApi,
		},
	},
];
