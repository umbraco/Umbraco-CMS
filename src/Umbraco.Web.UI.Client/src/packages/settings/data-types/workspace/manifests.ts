import { UmbSaveWorkspaceAction } from '@umbraco-cms/backoffice/workspace';
import type {
	ManifestWorkspace,
	ManifestWorkspaceAction,
	ManifestWorkspaceEditorView,
} from '@umbraco-cms/backoffice/extension-registry';

const DATA_TYPE_WORKSPACE_ALIAS = 'Umb.Workspace.DataType';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: DATA_TYPE_WORKSPACE_ALIAS,
	name: 'Data Type Workspace',
	loader: () => import('./data-type-workspace.element.js'),
	meta: {
		entityType: 'data-type',
	},
};

const workspaceViews: Array<ManifestWorkspaceEditorView> = [
	{
		type: 'workspaceEditorView',
		alias: 'Umb.WorkspaceView.DataType.Edit',
		name: 'Data Type Workspace Edit View',
		loader: () => import('./views/details/data-type-details-workspace-view.element.js'),
		weight: 90,
		meta: {
			label: 'Details',
			pathname: 'details',
			icon: 'edit',
			workspaces: [DATA_TYPE_WORKSPACE_ALIAS],
		},
	},
	{
		type: 'workspaceEditorView',
		alias: 'Umb.WorkspaceView.DataType.Info',
		name: 'Data Type Workspace Info View',
		loader: () => import('./views/info/workspace-view-data-type-info.element.js'),
		weight: 90,
		meta: {
			label: 'Info',
			pathname: 'info',
			icon: 'info',
			workspaces: [DATA_TYPE_WORKSPACE_ALIAS],
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
			workspaces: [DATA_TYPE_WORKSPACE_ALIAS],
		},
	},
];

export const manifests = [workspace, ...workspaceViews, ...workspaceActions];
