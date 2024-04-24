import type {
	ManifestTypes,
	ManifestWorkspace,
	ManifestWorkspaceActions,
	ManifestWorkspaceView,
} from '@umbraco-cms/backoffice/extension-registry';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: 'Umb.Workspace.PackageBuilder',
	name: 'Package Builder Workspace',
	element: () => import('./workspace/workspace-package-builder.element.js'),
	meta: {
		entityType: 'package-builder',
	},
};

const workspaceViews: Array<ManifestWorkspaceView> = [];
const workspaceActions: Array<ManifestWorkspaceActions> = [];

export const manifests: Array<ManifestTypes> = [workspace, ...workspaceViews, ...workspaceActions];
