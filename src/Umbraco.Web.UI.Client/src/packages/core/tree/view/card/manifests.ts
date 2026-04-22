import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.TreeView.Card',
		matchKind: 'card',
		matchType: 'treeView',
		manifest: {
			type: 'treeView',
			element: () => import('./card-tree-view.element.js'),
			weight: 900,
			meta: {
				label: '#tree_cardViewLabel',
				icon: 'icon-grid',
			},
		},
	},
];
