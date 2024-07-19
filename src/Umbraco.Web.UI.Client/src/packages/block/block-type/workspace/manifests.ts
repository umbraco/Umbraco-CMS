import { UMB_BLOCK_GRID_TYPE_WORKSPACE_ALIAS } from '../../block-grid/workspace/index.js';
import { UMB_BLOCK_LIST_TYPE_WORKSPACE_ALIAS } from '../../block-list/workspace/index.js';
import { UMB_BLOCK_RTE_TYPE_WORKSPACE_ALIAS } from '../../block-rte/workspace/index.js';
import { UmbSubmitWorkspaceAction } from '@umbraco-cms/backoffice/workspace';
import type { ManifestWorkspaceActions } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestWorkspaceActions> = [
	{
		type: 'workspaceAction',
		kind: 'default',
		alias: 'Umb.WorkspaceAction.BlockType.Save',
		name: 'Save Block Type Workspace Action',
		api: UmbSubmitWorkspaceAction,
		meta: {
			label: '#general_submit',
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
