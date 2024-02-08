import type {
	ManifestWorkspace,
	ManifestWorkspaceAction,
	ManifestWorkspaceView,
} from '@umbraco-cms/backoffice/extension-registry';

export const UMB_EXTENSION_ROOT_WORKSPACE_ALIAS = 'Umb.Workspace.ExtensionRoot';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: 'Umb.Workspace.ExtensionRoot',
	name: 'Extension Root Workspace',
	js: () => import('./extension-root-workspace.element.js'),
	meta: {
		entityType: 'extension-root',
	},
};

const workspaceViews: Array<ManifestWorkspaceView> = [];

const workspaceActions: Array<ManifestWorkspaceAction> = [];

export const manifests = [workspace, ...workspaceViews, ...workspaceActions];
