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
			element: () => import('./collection-create-action.element.js'),
			weight: 1200,
			meta: {
				label: '#actions_create',
			},
		},
	},
];
