import { manifests as workspaceViewManifests } from './views/manifests.js';
import { UMB_BLOCK_RTE_TYPE_WORKSPACE_ALIAS } from './index.js';

export const manifests: Array<UmbExtensionManifest> = [
	...workspaceViewManifests,
	{
		type: 'workspace',
		kind: 'routable',
		name: 'Block Rte Type Workspace',
		alias: UMB_BLOCK_RTE_TYPE_WORKSPACE_ALIAS,
		api: () => import('../../block-type/workspace/block-type-workspace.context.js'),
		meta: {
			entityType: 'block-rte-type',
		},
	},
];
