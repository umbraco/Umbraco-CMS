import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.TreeItemCard.Default',
	matchKind: 'default',
	matchType: 'treeItemCard',
	manifest: {
		type: 'treeItemCard',
		element: () => import('../../default/default-tree-item-card.element.js'),
		api: () => import('../../default/default-tree-item-card.api.js'),
	},
};
