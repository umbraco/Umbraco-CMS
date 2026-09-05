import { UMB_WEBHOOK_WORKSPACE_ALIAS } from '../../entity.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceContext',
		name: 'Webhook Menu Structure Workspace Context',
		alias: 'Umb.WorkspaceContext.Webhook.Menu.Structure',
		api: () => import('./webhook-menu-structure.context.js'),
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_WEBHOOK_WORKSPACE_ALIAS,
			},
		],
	},
	{
		type: 'workspaceFooterApp',
		kind: 'menuBreadcrumb',
		alias: 'Umb.WorkspaceFooterApp.Webhook.Breadcrumb',
		name: 'Webhook Breadcrumb Workspace Footer App',
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_WEBHOOK_WORKSPACE_ALIAS,
			},
		],
	},
];
