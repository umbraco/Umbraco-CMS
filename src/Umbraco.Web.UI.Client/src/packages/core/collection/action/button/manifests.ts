import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.CollectionAction.Button',
		matchKind: 'button',
		matchType: 'collectionAction',
		manifest: {
			type: 'collectionAction',
			kind: 'button',
			element: () => import('./collection-action-button.element.js'),
			api: () => import('./collection-action-button.api.js'),
		},
	},
];
