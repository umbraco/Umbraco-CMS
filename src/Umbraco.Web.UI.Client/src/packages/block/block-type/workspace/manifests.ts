import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'workspace',
		name: 'Block Workspace',
		alias: 'Umb.Workspace.BlockType',
		element: () => import('./block-type-workspace.element.js'),
		api: () => import('./block-type-workspace.context.js'),
		weight: 900,
		meta: {
			entityType: 'block-type',
		},
	},
];
