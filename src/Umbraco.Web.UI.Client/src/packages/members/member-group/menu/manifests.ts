import { UMB_MEMBER_GROUP_WORKSPACE_ALIAS } from '../workspace/member-group/constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceContext',
		name: 'Member Group Menu Structure Workspace Context',
		alias: 'Umb.WorkspaceContext.MemberGroup.Menu.Structure',
		api: () => import('./member-group-menu-structure.context.js'),
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_MEMBER_GROUP_WORKSPACE_ALIAS,
			},
		],
	},
	{
		type: 'workspaceFooterApp',
		kind: 'menuBreadcrumb',
		alias: 'Umb.WorkspaceFooterApp.MemberGroup.Breadcrumb',
		name: 'Member Group Breadcrumb Workspace Footer App',
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_MEMBER_GROUP_WORKSPACE_ALIAS,
			},
		],
	},
];
