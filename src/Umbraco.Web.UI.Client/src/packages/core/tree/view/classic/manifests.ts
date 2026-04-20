import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.TreeView.Classic',
		matchKind: 'classic',
		matchType: 'treeView',
		manifest: {
			type: 'treeView',
			element: () => import('./classic-tree-view.element.js'),
			weight: 1000,
			meta: {
				label: '#treeView_classic',
				icon: 'icon-blockquote',
			},
		},
	},
];
