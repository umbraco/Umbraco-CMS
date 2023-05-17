import { UmbSaveWorkspaceAction } from '@umbraco-cms/backoffice/workspace';
import type {
	ManifestWorkspace,
	ManifestWorkspaceAction,
	ManifestWorkspaceEditorView,
} from '@umbraco-cms/backoffice/extension-registry';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: 'Umb.Workspace.RelationType',
	name: 'Relation Type Workspace',
	loader: () => import('./relation-type-workspace.element'),
	meta: {
		entityType: 'relation-type',
	},
};

const workspaceViews: Array<ManifestWorkspaceEditorView> = [
	{
		type: 'workspaceEditorView',
		alias: 'Umb.WorkspaceView.RelationType.RelationType',
		name: 'Relation Type Workspace RelationType View',
		loader: () => import('./views/relation-type/relation-type-workspace-view-relation-type.element'),
		weight: 20,
		meta: {
			label: 'RelationType',
			pathname: 'relation-type',
			icon: 'umb:info',
		},
		conditions: {
			workspaces: ['Umb.Workspace.RelationType'],
		},
	},
	{
		type: 'workspaceEditorView',
		alias: 'Umb.WorkspaceView.RelationType.Relation',
		name: 'Relation Type Workspace Relation View',
		loader: () => import('./views/relation/workspace-view-relation-type-relation.element'),
		weight: 10,
		meta: {
			label: 'Relation',
			pathname: 'relation',
			icon: 'umb:trafic',
		},
		conditions: {
			workspaces: ['Umb.Workspace.RelationType'],
		},
	},
];

const workspaceActions: Array<ManifestWorkspaceAction> = [
	{
		type: 'workspaceAction',
		alias: 'Umb.WorkspaceAction.RelationType.Save',
		name: 'Save Relation Type Workspace Action',
		meta: {
			label: 'Save',
			look: 'primary',
			color: 'positive',
			api: UmbSaveWorkspaceAction,
		},
		conditions: {
			workspaces: ['Umb.Workspace.RelationType'],
		},
	},
];

export const manifests = [workspace, ...workspaceViews, ...workspaceActions];
