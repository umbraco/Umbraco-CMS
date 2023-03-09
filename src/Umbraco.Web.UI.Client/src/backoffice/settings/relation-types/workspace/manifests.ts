import { UmbSaveWorkspaceAction } from '@umbraco-cms/workspace';
import type { ManifestWorkspace, ManifestWorkspaceAction, ManifestWorkspaceView } from '@umbraco-cms/models';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: 'Umb.Workspace.RelationType',
	name: 'Data Type Workspace',
	loader: () => import('./relation-type-workspace.element'),
	meta: {
		entityType: 'relation-type',
	},
};

const workspaceViews: Array<ManifestWorkspaceView> = [
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.RelationType.Edit',
		name: 'Data Type Workspace Edit View',
		loader: () => import('./views/edit/relation-type-workspace-view-edit.element'),
		weight: 90,
		meta: {
			workspaces: ['Umb.Workspace.RelationType'],
			label: 'Edit',
			pathname: 'edit',
			icon: 'edit',
		},
	},
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.RelationType.Info',
		name: 'Data Type Workspace Info View',
		loader: () => import('./views/info/workspace-view-relation-type-info.element'),
		weight: 90,
		meta: {
			workspaces: ['Umb.Workspace.RelationType'],
			label: 'Info',
			pathname: 'info',
			icon: 'info',
		},
	},
];

const workspaceActions: Array<ManifestWorkspaceAction> = [
	{
		type: 'workspaceAction',
		alias: 'Umb.WorkspaceAction.RelationType.Save',
		name: 'Save Data Type Workspace Action',
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
