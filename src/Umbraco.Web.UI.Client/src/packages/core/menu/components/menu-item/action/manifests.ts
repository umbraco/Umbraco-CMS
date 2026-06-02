import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import UmbActionMenuItemApi from './action-menu-item.api.js';
import UmbActionMenuItemElement from './action-menu-item.element.js';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.MenuItem.Action',
		matchKind: 'action',
		matchType: 'menuItem',
		manifest: {
			type: 'menuItem',
			kind: 'action',
			api: UmbActionMenuItemApi,
			element: UmbActionMenuItemElement,
		},
	},
];
