import type { ManifestWorkspace, ManifestWorkspaceAction, ManifestWorkspaceView } from '@umbraco-cms/models';

const tree: ManifestWorkspace = {
	type: 'workspace',
	alias: 'Umb.Workspace.DocumentBlueprint.Root',
	name: 'Document Blueprint Root Workspace',
	loader: () => import('./document-blueprint-root-workspace.element'),
	meta: {
		entityType: 'document-blueprint-root',
	},
};

const workspaceViews: Array<ManifestWorkspaceView> = [];

const workspaceActions: Array<ManifestWorkspaceAction> = [];

export const manifests = [tree, ...workspaceViews, ...workspaceActions];
