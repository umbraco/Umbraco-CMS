import type {
	ManifestTypes,
	ManifestWorkspace,
	ManifestWorkspaceActions,
	ManifestWorkspaceView,
} from '@umbraco-cms/backoffice/extension-registry';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: 'Umb.Workspace.Package',
	name: 'Package Workspace',
	element: () => import('./workspace/workspace-package.element.js'),
	meta: {
		entityType: 'package',
	},
};

const workspaceViews: Array<ManifestWorkspaceView> = [];
const workspaceActions: Array<ManifestWorkspaceActions> = [];

export const manifests: Array<ManifestTypes> = [workspace, ...workspaceViews, ...workspaceActions];
