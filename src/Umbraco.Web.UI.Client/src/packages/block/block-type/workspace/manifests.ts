import { UMB_BLOCK_GRID_TYPE_WORKSPACE_ALIAS } from '../../block-grid/workspace/index.js';
import { UMB_BLOCK_LIST_TYPE_WORKSPACE_ALIAS } from '../../block-list/workspace/index.js';
import { UMB_BLOCK_RTE_TYPE_WORKSPACE_ALIAS } from '../../block-rte/workspace/index.js';
import { UMB_WORKSPACE_CONDITION_ALIAS, UmbSubmitWorkspaceAction } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
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
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				oneOf: [
					UMB_BLOCK_GRID_TYPE_WORKSPACE_ALIAS,
					UMB_BLOCK_LIST_TYPE_WORKSPACE_ALIAS,
					UMB_BLOCK_RTE_TYPE_WORKSPACE_ALIAS,
				],
			},
		],
	},
];
