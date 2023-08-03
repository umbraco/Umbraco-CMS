import type {
	ManifestWorkspace,
	ManifestWorkspaceAction,
	ManifestWorkspaceEditorView,
} from '@umbraco-cms/backoffice/extension-registry';

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
		conditions: {
			workspaces: ['Umb.Workspace.StyleSheet'],
		},
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
		conditions: {
			workspaces: ['Umb.Workspace.StyleSheet'],
		},
	},
];
const workspaceActions: Array<ManifestWorkspaceAction> = [];

export const manifests = [workspace, ...workspaceEditorViews, ...workspaceActions];
