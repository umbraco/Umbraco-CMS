import { UmbSaveWorkspaceAction } from '@umbraco-cms/backoffice/workspace';
import type {
	ManifestWorkspace,
	ManifestWorkspaceAction,
	ManifestWorkspaceView,
} from '@umbraco-cms/backoffice/extensions-registry';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: 'Umb.Workspace.DataType',
	name: 'Data Type Workspace',
	loader: () => import('./data-type-workspace.element'),
	meta: {
		entityType: 'data-type',
	},
};

const workspaceViews: Array<ManifestWorkspaceView> = [
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.DataType.Edit',
		name: 'Data Type Workspace Edit View',
		loader: () => import('./views/details/data-type-details-workspace-view.element'),
		weight: 90,
		meta: {
			label: 'Details',
			pathname: 'details',
			icon: 'edit',
		},
		conditions: {
			workspaces: ['Umb.Workspace.DataType'],
		},
	},
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.DataType.Info',
		name: 'Data Type Workspace Info View',
		loader: () => import('./views/info/workspace-view-data-type-info.element'),
		weight: 90,
		meta: {
			label: 'Info',
			pathname: 'info',
			icon: 'info',
		},
		conditions: {
			workspaces: ['Umb.Workspace.DataType'],
		},
	},
];

const workspaceActions: Array<ManifestWorkspaceAction> = [
	{
		type: 'workspaceAction',
		alias: 'Umb.WorkspaceAction.DataType.Save',
		name: 'Save Data Type Workspace Action',
		meta: {
			label: 'Save',
			look: 'primary',
			color: 'positive',
			api: UmbSaveWorkspaceAction,
		},
		conditions: {
			workspaces: ['Umb.Workspace.MemberGroup'],
		},
	},
];

export const manifests = [workspace, ...workspaceViews, ...workspaceActions];
