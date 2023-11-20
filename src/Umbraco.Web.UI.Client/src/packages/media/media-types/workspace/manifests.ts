import type {
	ManifestWorkspace,
	ManifestWorkspaceAction,
	ManifestWorkspaceEditorView,
	ManifestWorkspaceViewCollection,
} from '@umbraco-cms/backoffice/extension-registry';

import { UmbSaveWorkspaceAction } from '@umbraco-cms/backoffice/workspace';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: 'Umb.Workspace.MediaType',
	name: 'Media Type Workspace',
	js: () => import('./media-type-workspace.element.js'),
	meta: {
		entityType: 'media-type',
	},
};

const workspaceEditorViews: Array<ManifestWorkspaceEditorView> = [
	{
		type: 'workspaceEditorView',
		alias: 'Umb.WorkspaceView.MediaType.Design',
		name: 'Media Type Workspace Design View',
		js: () => import('./views/design/media-type-workspace-view-edit.element.js'),
		weight: 1000,
		meta: {
			label: 'Design',
			pathname: 'design',
			icon: 'icon-document-dashed-line',
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
		alias: 'Umb.WorkspaceView.MediaType.Structure',
		name: 'Media Type Workspace Structure View',
		js: () => import('./views/structure/media-type-workspace-view-structure.element.js'),
		weight: 800,
		meta: {
			label: 'Structure',
			pathname: 'structure',
			icon: 'icon-mindmap',
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
const workspaceActions: Array<ManifestWorkspaceAction> = [
	{
		type: 'workspaceAction',
		alias: 'Umb.WorkspaceAction.MediaType.Save',
		name: 'Save Media Type Workspace Action',
		api: UmbSaveWorkspaceAction,
		meta: {
			label: 'Save',
			look: 'primary',
			color: 'positive',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: workspace.alias,
			},
		],
	},
];

export const manifests = [workspace, ...workspaceEditorViews, ...workspaceViewCollections, ...workspaceActions];
