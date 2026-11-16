import { UMB_BLOCK_WORKSPACE_VARIES_BY_VARIANT_CONDITION_ALIAS } from '../../../conditions/constants.js';
import { UMB_BLOCK_WORKSPACE_MENU_ITEM_UNEXPOSE_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceActionMenuItem',
		kind: 'default',
		alias: UMB_BLOCK_WORKSPACE_MENU_ITEM_UNEXPOSE_ALIAS,
		name: 'Unexpose Block Workspace Action Menu Item',
		weight: 0,
		api: () => import('./unexpose.action.js'),
		forWorkspaceActions: 'Umb.WorkspaceAction.Block.SubmitUpdate',
		meta: {
			icon: '',
			label: '#blockEditor_setUnexpose',
		},
		conditions: [
			{ alias: UMB_BLOCK_WORKSPACE_VARIES_BY_VARIANT_CONDITION_ALIAS },
			{ alias: 'Umb.Condition.BlockWorkspaceIsReadOnly', match: false },
		],
	},
];
