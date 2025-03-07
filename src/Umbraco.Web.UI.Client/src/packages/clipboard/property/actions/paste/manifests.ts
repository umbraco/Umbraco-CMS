import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_PROPERTY_ACTION_DEFAULT_KIND_MANIFEST } from '@umbraco-cms/backoffice/property-action';

export const UMB_PROPERTY_ACTION_PASTE_FROM_CLIPBOARD_KIND_MANIFEST: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.PropertyAction.pasteFromClipboard',
	matchKind: 'pasteFromClipboard',
	matchType: 'propertyAction',
	manifest: {
		...UMB_PROPERTY_ACTION_DEFAULT_KIND_MANIFEST.manifest,
		type: 'propertyAction',
		kind: 'pasteFromClipboard',
		api: () => import('./paste-from-clipboard.property-action.js'),
		weight: 1190,
		meta: {
			icon: 'icon-clipboard-paste',
			label: 'Replace',
		},
	},
};

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	UMB_PROPERTY_ACTION_PASTE_FROM_CLIPBOARD_KIND_MANIFEST,
];
