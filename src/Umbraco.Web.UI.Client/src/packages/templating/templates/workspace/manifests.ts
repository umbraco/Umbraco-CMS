import { UmbSubmitWorkspaceAction, UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';
import { UMB_IS_SERVER_PRODUCTION_MODE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/server';

export const UMB_TEMPLATE_WORKSPACE_ALIAS = 'Umb.Workspace.Template';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		kind: 'routable',
		alias: UMB_TEMPLATE_WORKSPACE_ALIAS,
		name: 'Template Workspace',
		api: () => import('./template-workspace.context.js'),
		meta: {
			entityType: 'template',
		},
	},
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.Template.CodeEditor',
		name: 'Template Workspace Code Editor View',
		element: () => import('./views/code-editor/template-code-editor-workspace-view.element.js'),
		weight: 700,
		meta: {
			label: '#template_tabCode',
			pathname: 'code',
			icon: 'icon-brackets',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_TEMPLATE_WORKSPACE_ALIAS,
			},
		],
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
				alias: UMB_IS_SERVER_PRODUCTION_MODE_CONDITION_ALIAS,
				match: false,
			},
		],
	},
	{
		type: 'workspaceAction',
		alias: 'Umb.WorkspaceAction.Template.ProductionMode',
		name: 'Template Production Mode',
		api: () => import('../../local-components/production-mode-workspace-action/production-mode-workspace-action.js'),
		element: () =>
			import('../../local-components/production-mode-workspace-action/production-mode-workspace-action.js'),
		weight: 60,
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_TEMPLATE_WORKSPACE_ALIAS,
			},
			{
				alias: UMB_IS_SERVER_PRODUCTION_MODE_CONDITION_ALIAS,
				match: true,
			},
		],
	},
];
