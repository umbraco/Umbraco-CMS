import { UmbSubmitWorkspaceAction, UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';
import { UMB_IS_PRODUCTION_MODE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/server';

export const UMB_TEMPLATE_WORKSPACE_ALIAS = 'Umb.Workspace.Template';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		kind: 'routable',
		alias: 'Umb.Workspace.Template',
		name: 'Template Workspace',
		api: () => import('./template-workspace.context.js'),
		meta: {
			entityType: 'template',
		},
	},
	{
		type: 'workspaceAction',
		kind: 'default',
		alias: 'Umb.WorkspaceAction.Template.Save',
		name: 'Save Template',
		api: UmbSubmitWorkspaceAction,
		weight: 70,
		meta: {
			look: 'primary',
			color: 'positive',
			label: '#buttons_save',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_TEMPLATE_WORKSPACE_ALIAS,
			},
			{
				alias: UMB_IS_PRODUCTION_MODE_CONDITION_ALIAS,
				match: false,
			},
		],
	},
];
