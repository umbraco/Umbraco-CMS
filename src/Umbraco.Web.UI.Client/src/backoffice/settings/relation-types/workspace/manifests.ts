import { UmbSaveWorkspaceAction } from '@umbraco-cms/workspace';
import type { ManifestWorkspace, ManifestWorkspaceAction, ManifestWorkspaceView } from '@umbraco-cms/models';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: 'Umb.Workspace.RelationType',
	name: 'Relation Type Workspace',
	loader: () => import('./relation-type-workspace.element'),
	meta: {
		entityType: 'relation-type',
	},
};

const workspaceViews: Array<ManifestWorkspaceView> = [
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.RelationType.RelationType',
		name: 'Relation Type Workspace RelationType View',
		loader: () => import('./views/relation-type/relation-type-workspace-view-relation-type.element'),
		weight: 90,
		meta: {
			workspaces: ['Umb.Workspace.RelationType'],
			label: 'RelationType',
			pathname: 'relation-type',
			icon: 'umb:info',
		},
	},
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.RelationType.Relation',
		name: 'Relation Type Workspace Relation View',
		loader: () => import('./views/relation/workspace-view-relation-type-relation.element'),
		weight: 90,
		meta: {
			workspaces: ['Umb.Workspace.RelationType'],
			label: 'Relation',
			pathname: 'relation',
			icon: 'umb:trafic',
		},
	},
];

const workspaceActions: Array<ManifestWorkspaceAction> = [
	{
		type: 'workspaceAction',
		alias: 'Umb.WorkspaceAction.RelationType.Save',
		name: 'Save Relation Type Workspace Action',
		meta: {
			workspaces: ['Umb.Workspace.RelationType'],
			label: 'Save',
			look: 'primary',
			color: 'positive',
			api: UmbSaveWorkspaceAction,
		},
	},
];

export const manifests = [workspace, ...workspaceViews, ...workspaceActions];
