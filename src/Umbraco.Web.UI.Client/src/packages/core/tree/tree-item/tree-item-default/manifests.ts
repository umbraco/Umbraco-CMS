import type { ManifestTypes, UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

const kind: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.TreeItem.Default',
	matchKind: 'default',
	matchType: 'treeItem',
	manifest: {
		type: 'treeItem',
		api: () => import('./tree-item-default.context.js'),
		element: () => import('./tree-item-default.element.js'),
	},
};

export const manifests: Array<ManifestTypes | UmbExtensionManifestKind> = [kind];
