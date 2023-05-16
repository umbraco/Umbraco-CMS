import type {
	ManifestWorkspace,
	ManifestWorkspaceAction,
	ManifestWorkspaceEditorView,
} from '@umbraco-cms/backoffice/extension-registry';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: 'Umb.Workspace.StyleSheet',
	name: 'Stylesheet Workspace',
	loader: () => import('./stylesheet-workspace.element'),
	meta: {
		entityType: 'stylesheet',
	},
};

const workspaceViews: Array<ManifestWorkspaceEditorView> = [];
const workspaceActions: Array<ManifestWorkspaceAction> = [];

export const manifests = [workspace, ...workspaceViews, ...workspaceActions];
