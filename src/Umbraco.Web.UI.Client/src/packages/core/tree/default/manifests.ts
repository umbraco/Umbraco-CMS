import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.Tree.Default',
		matchKind: 'default',
		matchType: 'tree',
		manifest: {
			type: 'tree',
			api: () => import('./default-tree.context.js'),
			element: () => import('./default-tree.element.js'),
		},
	},
];
