import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_PROPERTY_ACTION_DEFAULT_KIND_MANIFEST } from '@umbraco-cms/backoffice/property-action';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.PropertyAction.Clipboard.Replace',
		matchKind: 'replaceFromClipboard',
		matchType: 'propertyAction',
		manifest: {
			...UMB_PROPERTY_ACTION_DEFAULT_KIND_MANIFEST.manifest,
			type: 'propertyAction',
			kind: 'replaceFromClipboard',
			api: () => import('./replace-from-clipboard.property-action.js'),
			weight: 1190,
			meta: {
				icon: 'icon-clipboard-paste',
				label: 'Replace',
			},
		},
	},
];
