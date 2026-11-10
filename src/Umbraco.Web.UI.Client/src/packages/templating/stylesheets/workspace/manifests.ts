import { UmbSubmitWorkspaceAction, UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const UMB_STYLESHEET_WORKSPACE_ALIAS = 'Umb.Workspace.Stylesheet';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		kind: 'routable',
		alias: UMB_STYLESHEET_WORKSPACE_ALIAS,
		name: 'Stylesheet Workspace',
		api: () => import('./stylesheet-workspace.context.js'),
		meta: {
			entityType: 'stylesheet',
		},
	},
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.Stylesheet.CodeEditor',
		name: 'Stylesheet Workspace Code Editor View',
		element: () => import('./views/code-editor/stylesheet-code-editor-workspace-view.element.js'),
		weight: 700,
		meta: {
			label: '#stylesheet_tabCode',
			pathname: 'code',
			icon: 'icon-brackets',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_STYLESHEET_WORKSPACE_ALIAS,
			},
		],
	},
	{
		type: 'workspaceAction',
		kind: 'default',
		alias: 'Umb.WorkspaceAction.Stylesheet.Save',
		name: 'Save Stylesheet Workspace Action',
		api: UmbSubmitWorkspaceAction,
		meta: {
			label: '#buttons_save',
			look: 'primary',
			color: 'positive',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_STYLESHEET_WORKSPACE_ALIAS,
			},
		],
	},
];
