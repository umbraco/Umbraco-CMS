import type {
	ManifestTypes,
	ManifestWorkspace,
	ManifestWorkspaceActions,
	ManifestWorkspaceView,
} from '@umbraco-cms/backoffice/extension-registry';

export const UMB_EXTENSION_ROOT_WORKSPACE_ALIAS = 'Umb.Workspace.ExtensionRoot';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: 'Umb.Workspace.ExtensionRoot',
	name: 'Extension Root Workspace',
	element: () => import('./extension-root-workspace.element.js'),
	meta: {
		entityType: 'extension-root',
	},
};

const workspaceViews: Array<ManifestWorkspaceView> = [];

const workspaceActions: Array<ManifestWorkspaceActions> = [];

export const manifests: Array<ManifestTypes> = [workspace, ...workspaceViews, ...workspaceActions];
