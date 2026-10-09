import UmbCardTreeViewElement from './card-tree-view.element.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.TreeView.Card',
		matchKind: 'card',
		matchType: 'treeView',
		manifest: {
			type: 'treeView',
			element: UmbCardTreeViewElement,
			weight: 800,
			meta: {
				label: '#tree_cardViewLabel',
				icon: 'icon-grid',
			},
		},
	},
];
