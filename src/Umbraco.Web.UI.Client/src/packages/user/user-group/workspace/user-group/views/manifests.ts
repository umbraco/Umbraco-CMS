import { UMB_USER_GROUP_WORKSPACE_ALIAS } from '../constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.UserGroup.Details',
		name: 'User Group Details Workspace View',
		element: () => import('./user-group-details-workspace-view.element.js'),
		weight: 90,
		meta: {
			label: '#general_details',
			pathname: 'details',
			icon: 'edit',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_USER_GROUP_WORKSPACE_ALIAS,
			},
		],
	},
];
