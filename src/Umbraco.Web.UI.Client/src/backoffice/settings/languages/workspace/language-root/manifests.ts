import type {
	ManifestWorkspace,
	ManifestWorkspaceAction,
	ManifestWorkspaceEditorView,
} from '@umbraco-cms/backoffice/extension-registry';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: 'Umb.Workspace.LanguageRoot',
	name: 'Language Root Workspace',
	loader: () => import('./language-root-workspace.element'),
	meta: {
		entityType: 'language-root',
	},
};

const workspaceViews: Array<ManifestWorkspaceEditorView> = [];

const workspaceActions: Array<ManifestWorkspaceAction> = [];

export const manifests = [workspace, ...workspaceViews, ...workspaceActions];
