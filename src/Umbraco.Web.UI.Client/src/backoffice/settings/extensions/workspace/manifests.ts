import type {
	ManifestWorkspace,
	ManifestWorkspaceAction,
	ManifestWorkspaceEditorView,
} from '@umbraco-cms/backoffice/extension-registry';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: 'Umb.Workspace.ExtensionRoot',
	name: 'Extension Root Workspace',
	loader: () => import('./extension-root-workspace.element'),
	meta: {
		entityType: 'extension-root',
	},
};

const workspaceViews: Array<ManifestWorkspaceEditorView> = [];

const workspaceActions: Array<ManifestWorkspaceAction> = [];

export const manifests = [workspace, ...workspaceViews, ...workspaceActions];
