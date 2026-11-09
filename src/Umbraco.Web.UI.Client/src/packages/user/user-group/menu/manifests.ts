import { UMB_USER_GROUP_WORKSPACE_ALIAS } from '../workspace/user-group/constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceContext',
		name: 'User Group Menu Structure Workspace Context',
		alias: 'Umb.WorkspaceContext.UserGroup.Menu.Structure',
		api: () => import('./user-group-menu-structure.context.js'),
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_USER_GROUP_WORKSPACE_ALIAS,
			},
		],
	},
	{
		type: 'workspaceFooterApp',
		kind: 'menuBreadcrumb',
		alias: 'Umb.WorkspaceFooterApp.UserGroup.Breadcrumb',
		name: 'User Group Breadcrumb Workspace Footer App',
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_USER_GROUP_WORKSPACE_ALIAS,
			},
		],
	},
];
