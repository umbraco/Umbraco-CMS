import type {
	ManifestWorkspace,
	ManifestWorkspaceAction,
	ManifestWorkspaceView,
	ManifestWorkspaceViewCollection,
} from '@umbraco-cms/models';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: 'Umb.Workspace.Package',
	name: 'Package Workspace',
	loader: () => import('./workspace/workspace-package.element'),
	meta: {
		entityType: 'package',
	},
};

const workspaceViews: Array<ManifestWorkspaceView> = [];
const workspaceViewCollections: Array<ManifestWorkspaceViewCollection> = [];
const workspaceActions: Array<ManifestWorkspaceAction> = [];

export const manifests = [workspace, ...workspaceViews, ...workspaceViewCollections, ...workspaceActions];
