import { manifests as workspaceViewManifests } from './views/manifests.js';
import { UMB_BLOCK_LIST_TYPE_WORKSPACE_ALIAS } from './index.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	...workspaceViewManifests,
	{
		type: 'workspace',
		name: 'Block List Type Workspace',
		alias: UMB_BLOCK_LIST_TYPE_WORKSPACE_ALIAS,
		element: () => import('../../block-type/workspace/block-type-workspace.element.js'),
		api: () => import('../../block-type/workspace/block-type-workspace.context.js'),
		weight: 900,
		meta: {
			entityType: 'block-list-type',
		},
	},
];
