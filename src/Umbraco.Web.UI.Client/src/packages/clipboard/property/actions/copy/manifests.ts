import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_PROPERTY_ACTION_DEFAULT_KIND_MANIFEST } from '@umbraco-cms/backoffice/property-action';
import { UMB_ACTION_GROUP_CLIPBOARD } from '@umbraco-cms/backoffice/action';

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
			group: UMB_ACTION_GROUP_CLIPBOARD,
			api: () => import('./copy-to-clipboard.property-action.js'),
			weight: 1200,
			meta: {
				icon: 'icon-clipboard-copy',
				label: '#general_copy',
			},
		},
	},
];
