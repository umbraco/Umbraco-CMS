import type {
	ManifestWorkspace,
	ManifestWorkspaceAction,
	ManifestWorkspaceEditorView,
	ManifestWorkspaceViewCollection,
} from '@umbraco-cms/backoffice/extension-registry';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: 'Umb.Workspace.PackageBuilder',
	name: 'Package Builder Workspace',
	loader: () => import('./workspace/workspace-package-builder.element'),
	meta: {
		entityType: 'package-builder',
	},
};

const workspaceViews: Array<ManifestWorkspaceEditorView> = [];
const workspaceViewCollections: Array<ManifestWorkspaceViewCollection> = [];
const workspaceActions: Array<ManifestWorkspaceAction> = [];

export const manifests = [workspace, ...workspaceViews, ...workspaceViewCollections, ...workspaceActions];
