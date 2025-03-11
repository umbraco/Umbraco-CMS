import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.PropertyContext.Clipboard',
		matchKind: 'clipboard',
		matchType: 'propertyContext',
		manifest: {
			type: 'propertyContext',
			kind: 'clipboard',
			api: () => import('./clipboard.property-context.js'),
			weight: 1200,
		},
	},
];
