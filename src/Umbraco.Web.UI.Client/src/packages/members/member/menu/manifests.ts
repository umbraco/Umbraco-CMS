import { UMB_MEMBER_WORKSPACE_ALIAS } from '../workspace/member/constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceContext',
		name: 'Member Menu Structure Workspace Context',
		alias: 'Umb.WorkspaceContext.Member.Menu.Structure',
		api: () => import('./member-menu-structure.context.js'),
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_MEMBER_WORKSPACE_ALIAS,
			},
		],
	},
	{
		type: 'workspaceFooterApp',
		kind: 'variantMenuBreadcrumb',
		alias: 'Umb.WorkspaceFooterApp.Member.Breadcrumb',
		name: 'Member Breadcrumb Workspace Footer App',
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_MEMBER_WORKSPACE_ALIAS,
			},
		],
	},
];
