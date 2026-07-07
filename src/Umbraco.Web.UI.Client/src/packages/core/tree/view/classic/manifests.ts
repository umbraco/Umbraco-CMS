import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import UmbClassicTreeViewElement from './classic-tree-view.element.js';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.TreeView.Classic',
		matchKind: 'classic',
		matchType: 'treeView',
		manifest: {
			type: 'treeView',
			element: UmbClassicTreeViewElement,
			weight: 1000,
			meta: {
				label: '#tree_classicViewLabel',
				icon: 'icon-blockquote',
			},
		},
	},
];
