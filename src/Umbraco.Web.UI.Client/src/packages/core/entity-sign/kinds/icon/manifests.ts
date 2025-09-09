import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.EntitySign.Icon',
		matchKind: 'icon',
		matchType: 'entitySign',
		manifest: {
			type: 'entitySign',
			kind: 'icon',
			element: () => import('./entity-sign-icon.element.js'),
			api: () => import('./entity-sign-icon.api.js'),
		},
	},
];
