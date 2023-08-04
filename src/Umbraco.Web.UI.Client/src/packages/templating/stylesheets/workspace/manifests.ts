import type {
	ManifestWorkspace,
	ManifestWorkspaceAction,
	ManifestWorkspaceEditorView,
} from '@umbraco-cms/backoffice/extension-registry';
import { UmbSaveWorkspaceAction } from '@umbraco-cms/backoffice/workspace';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: 'Umb.Workspace.StyleSheet',
	name: 'Stylesheet Workspace',
	loader: () => import('./stylesheet-workspace.element.js'),
	meta: {
		entityType: 'stylesheet',
	},
};

const workspaceEditorViews: Array<ManifestWorkspaceEditorView> = [
	{
		type: 'workspaceEditorView',
		alias: 'Umb.WorkspaceView.Stylesheet.CodeEditor',
		name: 'Stylesheet Workspace Code Editor View',
		loader: () => import('./views/code-editor/stylesheet-workspace-view-code-editor.element.js'),
		weight: 700,
		meta: {
			label: 'Code',
			pathname: 'code',
			icon: 'umb:brackets',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: 'Umb.Workspace.StyleSheet',
			},
		],
	},
	{
		type: 'workspaceEditorView',
		alias: 'Umb.WorkspaceView.Stylesheet.RichTextEditor',
		name: 'Stylesheet Workspace Rich Text Editor View',
		loader: () => import('./views/rich-text-editor/stylesheet-workspace-view-rich-text-editor.element.js'),
		weight: 800,
		meta: {
			label: 'Rich Text Editor',
			pathname: 'rich-text-editor',
			icon: 'umb:font',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: 'Umb.Workspace.StyleSheet',
			},
		],
	},
];
const workspaceActions: Array<ManifestWorkspaceAction> = [
	{
		type: 'workspaceAction',
		alias: 'Umb.WorkspaceAction.Stylesheet.Save',
		name: 'Save Stylesheet Workspace Action',
		meta: {
			label: 'Save',
			look: 'primary',
			color: 'positive',
			api: UmbSaveWorkspaceAction,
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: 'Umb.Workspace.StyleSheet',
			},
		],
	},
];

export const manifests = [workspace, ...workspaceEditorViews, ...workspaceActions];
