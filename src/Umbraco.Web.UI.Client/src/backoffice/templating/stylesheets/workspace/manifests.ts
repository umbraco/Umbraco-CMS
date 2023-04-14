import type {
	ManifestWorkspace,
	ManifestWorkspaceAction,
	ManifestWorkspaceView,
} from '@umbraco-cms/backoffice/extensions-registry';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: 'Umb.Workspace.StyleSheet',
	name: 'Stylesheet Workspace',
	loader: () => import('./stylesheet-workspace.element'),
	meta: {
		entityType: 'stylesheet',
	},
};

const workspaceViews: Array<ManifestWorkspaceView> = [];
const workspaceActions: Array<ManifestWorkspaceAction> = [];

export const manifests = [workspace, ...workspaceViews, ...workspaceActions];
