import UmbMenuItemTreeDefaultElement from './tree-menu-item.element.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.Tree',
		matchKind: 'tree',
		matchType: 'menuItem',
		manifest: {
			type: 'menuItem',
			element: UmbMenuItemTreeDefaultElement,
		},
	},
];
