import { manifests as workspaceViewManifests } from './views/manifests.js';
import { UMB_BLOCK_GRID_AREA_TYPE_WORKSPACE_ALIAS } from './index.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';
import { UmbSaveWorkspaceAction } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<ManifestTypes> = [
	...workspaceViewManifests,
	{
		type: 'workspace',
		name: 'Block Grid Area Type Workspace',
		alias: UMB_BLOCK_GRID_AREA_TYPE_WORKSPACE_ALIAS,
		element: () => import('./block-grid-area-type-workspace.element.js'),
		api: () => import('./block-grid-area-type-workspace.context.js'),
		meta: {
			entityType: 'block-grid-area-type',
		},
	},
	{
		type: 'workspaceAction',
		alias: 'Umb.WorkspaceAction.BlockGridAreaType.Save',
		name: 'Save Block Grid Area Type Workspace Action',
		api: UmbSaveWorkspaceAction,
		meta: {
			label: 'Submit',
			look: 'primary',
			color: 'positive',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: UMB_BLOCK_GRID_AREA_TYPE_WORKSPACE_ALIAS,
			},
		],
	},
];
