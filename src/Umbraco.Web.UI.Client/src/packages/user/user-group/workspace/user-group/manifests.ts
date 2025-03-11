import { UMB_USER_GROUP_WORKSPACE_ALIAS } from './constants.js';
import { UmbSubmitWorkspaceAction, UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		kind: 'routable',
		alias: UMB_USER_GROUP_WORKSPACE_ALIAS,
		name: 'User Group Workspace',
		api: () => import('./user-group-workspace.context.js'),
		meta: {
			entityType: 'user-group',
		},
	},
	{
		type: 'workspaceAction',
		kind: 'default',
		alias: 'Umb.WorkspaceAction.UserGroup.Save',
		name: 'Save User Group Workspace Action',
		api: UmbSubmitWorkspaceAction,
		meta: {
			label: '#buttons_save',
			look: 'primary',
			color: 'positive',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_USER_GROUP_WORKSPACE_ALIAS,
			},
		],
	},
];
