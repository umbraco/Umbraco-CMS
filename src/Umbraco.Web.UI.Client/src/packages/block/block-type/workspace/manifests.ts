import { UMB_BLOCK_TYPE_WORKSPACE_ALIAS } from './block-type-workspace-alias.const.js';
import { manifests as workspaceViewManifests } from './views/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	...workspaceViewManifests,
	{
		type: 'workspace',
		name: 'Block Workspace',
		alias: UMB_BLOCK_TYPE_WORKSPACE_ALIAS,
		element: () => import('./block-type-workspace.element.js'),
		api: () => import('./block-type-workspace.context.js'),
		weight: 900,
		meta: {
			entityType: 'block-type',
		},
	},
];
