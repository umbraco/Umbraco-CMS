import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.DefaultTreeItem',
		matchKind: 'default',
		matchType: 'treeItem',
		manifest: {
			type: 'treeItem',
			element: () => import('./tree-item-default.element.js'),
			api: () => import('./tree-item-default.context.js'),
		},
	},
];
