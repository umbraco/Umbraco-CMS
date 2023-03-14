import { UmbSaveWorkspaceAction } from '@umbraco-cms/workspace';
import type { ManifestWorkspace, ManifestWorkspaceAction, ManifestWorkspaceView } from '@umbraco-cms/models';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: 'Umb.Workspace.Language',
	name: 'Language Workspace',
	loader: () => import('./language-workspace.element'),
	meta: {
		entityType: 'language',
		api: () => import('./language-workspace.context'),
	},
};

const workspaceViews: Array<ManifestWorkspaceView> = [
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.Language.Details',
		name: 'Language Workspace Details View',
		loader: () => import('./views/details/language-details-workspace-view.element'),
		weight: 90,
		meta: {
			workspaces: ['Umb.Workspace.Language'],
			label: 'Details',
			pathname: 'details',
			icon: 'edit',
		},
	},
];

const workspaceActions: Array<ManifestWorkspaceAction> = [
	{
		type: 'workspaceAction',
		alias: 'Umb.WorkspaceAction.Language.Save',
		name: 'Save Language Workspace Action',
		meta: {
			workspaces: ['Umb.Workspace.Language'],
			look: 'primary',
			color: 'positive',
			label: 'Save',
			api: UmbSaveWorkspaceAction,
		},
	},
];

export const manifests = [workspace, ...workspaceViews, ...workspaceActions];
