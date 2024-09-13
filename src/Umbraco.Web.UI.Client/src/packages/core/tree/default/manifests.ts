import type { ManifestTypes, UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

const defaultTreeKind: UmbExtensionManifestKind = {
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

export const manifests: Array<ManifestTypes | UmbExtensionManifestKind> = [defaultTreeKind];
