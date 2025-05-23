import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_PROPERTY_ACTION_DEFAULT_KIND_MANIFEST } from '@umbraco-cms/backoffice/property-action';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.PropertyAction.CopyToClipboard',
		matchKind: 'copyToClipboard',
		matchType: 'propertyAction',
		manifest: {
			...UMB_PROPERTY_ACTION_DEFAULT_KIND_MANIFEST.manifest,
			type: 'propertyAction',
			kind: 'copyToClipboard',
			api: () => import('./copy-to-clipboard.property-action.js'),
			weight: 1200,
			meta: {
				icon: 'icon-clipboard-copy',
				label: 'Copy',
			},
		},
	},
];
