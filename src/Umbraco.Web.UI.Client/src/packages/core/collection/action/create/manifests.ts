import { UmbCollectionCreateActionButtonElement } from './collection-create-action.element.js';
import { UmbCreateCollectionActionApi } from './collection-create-action.api.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.CollectionAction.Create',
		matchKind: 'create',
		matchType: 'collectionAction',
		manifest: {
			type: 'collectionAction',
			kind: 'create',
			api: UmbCreateCollectionActionApi,
			element: UmbCollectionCreateActionButtonElement,
			weight: 1200,
			meta: {
				label: '#actions_createFor',
			},
		},
	},
];
