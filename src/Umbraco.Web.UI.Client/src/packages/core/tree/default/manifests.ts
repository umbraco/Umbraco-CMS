import type { UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';

const defaultTreeKind: UmbBackofficeManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.Tree.Default',
	matchKind: 'default',
	matchType: 'tree',
	manifest: {
		type: 'tree',
		api: () => import('./default-tree.context.js'),
		element: () => import('./default-tree.element.js'),
	},
};

export const manifests = [defaultTreeKind];
