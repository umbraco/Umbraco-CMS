import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { UmbTableTreeViewElement } from './table-tree-view.element.js';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.TreeView.Table',
		matchKind: 'table',
		matchType: 'treeView',
		manifest: {
			type: 'treeView',
			element: UmbTableTreeViewElement,
			weight: 850,
			meta: {
				label: '#tree_tableViewLabel',
				icon: 'icon-table',
			},
		},
	},
];
