import type {
	ManifestWorkspace,
	ManifestWorkspaceAction,
	ManifestWorkspaceEditorView,
	ManifestWorkspaceViewCollection,
} from '@umbraco-cms/backoffice/extension-registry';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: 'Umb.Workspace.MediaType',
	name: 'Media Type Workspace',
	loader: () => import('./media-type-workspace.element.js'),
	meta: {
		entityType: 'media-type',
	},
};

const workspaceViews: Array<ManifestWorkspaceEditorView> = [
	{
		type: 'workspaceEditorView',
		alias: 'Umb.WorkspaceView.MediaType.Design',
		name: 'Media Type Workspace Design View',
		loader: () => import('./views/details/media-type-design-workspace-view.element.js'),
		weight: 90,
		meta: {
			label: 'Details',
			pathname: 'details',
			icon: 'document',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: workspace.alias,
			},
		],
	},
	{
		type: 'workspaceEditorView',
		alias: 'Umb.WorkspaceView.MediaType.ListView',
		name: 'Media Type Workspace ListView View',
		loader: () => import('./views/details/media-type-list-view-workspace-view.element.js'),
		weight: 90,
		meta: {
			label: 'List View',
			pathname: 'list-view',
			icon: 'bug',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: workspace.alias,
			},
		],
	},
	{
		type: 'workspaceEditorView',
		alias: 'Umb.WorkspaceView.MediaType.Permissions',
		name: 'Media Type Workspace Permissions View',
		loader: () => import('./views/details/media-type-permissions-workspace-view.element.js'),
		weight: 90,
		meta: {
			label: 'Permissions',
			pathname: 'permissions',
			icon: 'bug',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: workspace.alias,
			},
		],
	},
];
const workspaceViewCollections: Array<ManifestWorkspaceViewCollection> = [];
const workspaceActions: Array<ManifestWorkspaceAction> = [];

export const manifests = [workspace, ...workspaceViews, ...workspaceViewCollections, ...workspaceActions];
