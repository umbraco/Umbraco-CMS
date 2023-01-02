import type { ManifestWorkspace, ManifestWorkspaceAction, ManifestWorkspaceView } from '@umbraco-cms/models';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: 'Umb.Workspace.DataType',
	name: 'Data Type Workspace',
	loader: () => import('./workspace-data-type.element'),
	meta: {
		entityType: 'data-type',
	},
};

const workspaceViews: Array<ManifestWorkspaceView> = [
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.DataType.Edit',
		name: 'Data Type Workspace Edit View',
		loader: () => import('./views/edit/workspace-view-data-type-edit.element'),
		weight: 90,
		meta: {
			workspaces: ['Umb.Workspace.DataType'],
			label: 'Edit',
			pathname: 'edit',
			icon: 'edit',
		},
	},
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.DataType.Info',
		name: 'Data Type Workspace Info View',
		loader: () => import('./views/info/workspace-view-data-type-info.element'),
		weight: 90,
		meta: {
			workspaces: ['Umb.Workspace.DataType'],
			label: 'Info',
			pathname: 'info',
			icon: 'info',
		},
	},
];

const workspaceActions: Array<ManifestWorkspaceAction> = [
	{
		type: 'workspaceAction',
		alias: 'Umb.WorkspaceAction.DataType.Save',
		name: 'Save Data Type Workspace Action',
		loader: () => import('src/backoffice/core/components/workspace/actions/save/workspace-action-node-save.element'),
		meta: {
			workspaces: ['Umb.Workspace.DataType'],
			look: 'primary',
			color: 'positive',
		},
	},
];

export const manifests = [workspace, ...workspaceViews, ...workspaceActions];
