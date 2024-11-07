import { UMB_SCRIPT_ENTITY_TYPE } from '../entity.js';
import { UMB_WORKSPACE_CONDITION_ALIAS, UmbSubmitWorkspaceAction } from '@umbraco-cms/backoffice/workspace';

export const UMB_SCRIPT_WORKSPACE_ALIAS = 'Umb.Workspace.Script';
export const UMB_SAVE_SCRIPT_WORKSPACE_ACTION_ALIAS = 'Umb.WorkspaceAction.Script.Save';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		kind: 'routable',
		alias: UMB_SCRIPT_WORKSPACE_ALIAS,
		name: 'Script Workspace',
		api: () => import('./script-workspace.context.js'),
		meta: {
			entityType: UMB_SCRIPT_ENTITY_TYPE,
		},
	},
	{
		type: 'workspaceAction',
		kind: 'default',
		alias: UMB_SAVE_SCRIPT_WORKSPACE_ACTION_ALIAS,
		name: 'Save Script Workspace Action',
		api: UmbSubmitWorkspaceAction,
		meta: {
			label: '#buttons_save',
			look: 'primary',
			color: 'positive',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_SCRIPT_WORKSPACE_ALIAS,
			},
		],
	},
];
