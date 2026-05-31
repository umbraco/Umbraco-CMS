import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { UmbCollectionCreateActionButtonElement } from './collection-create-action.element.js';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.CollectionAction.Create',
		matchKind: 'create',
		matchType: 'collectionAction',
		manifest: {
			type: 'collectionAction',
			kind: 'create',
			element: UmbCollectionCreateActionButtonElement,
			weight: 1200,
			meta: {
				label: '#actions_createFor',
			},
		},
	},
];
