import type {
	ManifestWorkspace,
	ManifestWorkspaceActions,
	ManifestWorkspaceView,
} from '@umbraco-cms/backoffice/extension-registry';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: 'Umb.Workspace.Package',
	name: 'Package Workspace',
	js: () => import('./workspace/workspace-package.element.js'),
	meta: {
		entityType: 'package',
	},
};

const workspaceViews: Array<ManifestWorkspaceView> = [];
const workspaceActions: Array<ManifestWorkspaceActions> = [];

export const manifests = [workspace, ...workspaceViews, ...workspaceActions];
