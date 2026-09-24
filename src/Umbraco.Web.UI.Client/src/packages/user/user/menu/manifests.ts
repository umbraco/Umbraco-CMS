import { UMB_USER_WORKSPACE_ALIAS } from '../workspace/user/constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceContext',
		name: 'User Menu Structure Workspace Context',
		alias: 'Umb.WorkspaceContext.User.Menu.Structure',
		api: () => import('./user-menu-structure.context.js'),
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_USER_WORKSPACE_ALIAS,
			},
		],
	},
	{
		type: 'workspaceFooterApp',
		kind: 'menuBreadcrumb',
		alias: 'Umb.WorkspaceFooterApp.User.Breadcrumb',
		name: 'User Breadcrumb Workspace Footer App',
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_USER_WORKSPACE_ALIAS,
			},
		],
	},
];
