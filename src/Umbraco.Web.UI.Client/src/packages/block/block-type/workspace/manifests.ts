import { UMB_BLOCK_GRID_TYPE_WORKSPACE_ALIAS } from '../../block-grid/workspace/index.js';
import { UMB_BLOCK_LIST_TYPE_WORKSPACE_ALIAS } from '../../block-list/workspace/index.js';
import { UMB_BLOCK_RTE_TYPE_WORKSPACE_ALIAS } from '../../block-rte/workspace/index.js';
import { UmbSaveWorkspaceAction } from '@umbraco-cms/backoffice/workspace';
import type { ManifestWorkspaceAction } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestWorkspaceAction> = [
	{
		type: 'workspaceAction',
		alias: 'Umb.WorkspaceAction.BlockType.Save',
		name: 'Save Block Type Workspace Action',
		api: UmbSaveWorkspaceAction,
		meta: {
			label: 'Submit',
			look: 'primary',
			color: 'positive',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				oneOf: [
					UMB_BLOCK_GRID_TYPE_WORKSPACE_ALIAS,
					UMB_BLOCK_LIST_TYPE_WORKSPACE_ALIAS,
					UMB_BLOCK_RTE_TYPE_WORKSPACE_ALIAS,
				],
			},
		],
	},
];
