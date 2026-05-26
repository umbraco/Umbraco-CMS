import { UMB_CURRENT_USER_ENTITY_TYPE } from '../entity.js';
import { UMB_CURRENT_USER_WORKSPACE_ALIAS } from './constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS, UmbSubmitWorkspaceAction } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		alias: UMB_CURRENT_USER_WORKSPACE_ALIAS,
		name: 'Current User Workspace',
		element: () => import('./current-user-workspace-editor.element.js'),
		api: () => import('./current-user-workspace.context.js'),
		meta: {
			entityType: UMB_CURRENT_USER_ENTITY_TYPE,
		},
	},
	{
		type: 'workspaceAction',
		kind: 'default',
		alias: 'Umb.WorkspaceAction.CurrentUser.Save',
		name: 'Save Current User Workspace Action',
		api: UmbSubmitWorkspaceAction,
		meta: {
			label: '#buttons_save',
			look: 'primary',
			color: 'positive',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_CURRENT_USER_WORKSPACE_ALIAS,
			},
		],
	},
];
