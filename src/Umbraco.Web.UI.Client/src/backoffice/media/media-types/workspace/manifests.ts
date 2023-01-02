import type {
	ManifestWorkspace,
	ManifestWorkspaceAction,
	ManifestWorkspaceView,
	ManifestWorkspaceViewCollection,
} from '@umbraco-cms/models';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: 'Umb.Workspace.MediaType',
	name: 'Media Type Workspace',
	loader: () => import('./media-type-workspace.element'),
	meta: {
		entityType: 'media-type',
	},
};

const workspaceViews: Array<ManifestWorkspaceView> = [];
const workspaceViewCollections: Array<ManifestWorkspaceViewCollection> = [];
const workspaceActions: Array<ManifestWorkspaceAction> = [];

export const manifests = [workspace, ...workspaceViews, ...workspaceViewCollections, ...workspaceActions];
