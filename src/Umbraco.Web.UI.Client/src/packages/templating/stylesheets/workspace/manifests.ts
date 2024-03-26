import type {
	ManifestWorkspaces,
	ManifestWorkspaceActions,
	ManifestWorkspaceView,
} from '@umbraco-cms/backoffice/extension-registry';
import { UmbSaveWorkspaceAction } from '@umbraco-cms/backoffice/workspace';

export const UMB_STYLESHEET_WORKSPACE_ALIAS = 'Umb.Workspace.Stylesheet';

const workspace: ManifestWorkspaces = {
	type: 'workspace',
	kind: 'routable',
	alias: UMB_STYLESHEET_WORKSPACE_ALIAS,
	name: 'Stylesheet Workspace',
	api: () => import('./stylesheet-workspace.context.js'),
	meta: {
		entityType: 'stylesheet',
	},
};

const workspaceViews: Array<ManifestWorkspaceView> = [
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.Stylesheet.CodeEditor',
		name: 'Stylesheet Workspace Code Editor View',
		js: () => import('./views/code-editor/stylesheet-code-editor-workspace-view.element.js'),
		weight: 700,
		meta: {
			label: 'Code',
			pathname: 'code',
			icon: 'icon-brackets',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: workspace.alias,
			},
		],
	},
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.Stylesheet.RichTextEditor',
		name: 'Stylesheet Workspace Rich Text Editor View',
		js: () => import('./views/rich-text-rule/stylesheet-rich-text-rule-workspace-view.element.js'),
		weight: 800,
		meta: {
			label: 'Rich Text Editor',
			pathname: 'rich-text-editor',
			icon: 'icon-font',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: workspace.alias,
			},
		],
	},
];
const workspaceActions: Array<ManifestWorkspaceActions> = [
	{
		type: 'workspaceAction',
		kind: 'default',
		alias: 'Umb.WorkspaceAction.Stylesheet.Save',
		name: 'Save Stylesheet Workspace Action',
		api: UmbSaveWorkspaceAction,
		meta: {
			label: 'Save',
			look: 'primary',
			color: 'positive',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: workspace.alias,
			},
		],
	},
];

export const manifests = [workspace, ...workspaceViews, ...workspaceActions];
