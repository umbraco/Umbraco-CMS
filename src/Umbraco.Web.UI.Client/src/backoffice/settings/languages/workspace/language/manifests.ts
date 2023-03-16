import { LANGUAGE_REPOSITORY_ALIAS } from '../../repository/manifests';
import { UmbSaveWorkspaceAction } from '@umbraco-cms/workspace';
import type { ManifestWorkspace, ManifestWorkspaceAction, ManifestWorkspaceView } from '@umbraco-cms/models';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: 'Umb.Workspace.Language',
	name: 'Language Workspace',
	loader: () => import('./language-workspace.element'),
	meta: {
		entityType: 'language',
	},
};

const workspaceViews: Array<ManifestWorkspaceView> = [
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.Language.Edit',
		name: 'Language Workspace Edit View',
		loader: () => import('./views/edit/edit-language-workspace-view.element'),
		weight: 90,
		meta: {
			label: 'Edit',
			pathname: 'edit',
			icon: 'edit',
		},
		conditions: {
			workspaces: ['Umb.Workspace.Language'],
		},
	},
];

const workspaceActions: Array<ManifestWorkspaceAction> = [
	{
		type: 'workspaceAction',
		alias: 'Umb.WorkspaceAction.Language.Save',
		name: 'Save Language Workspace Action',
		meta: {
			look: 'primary',
			color: 'positive',
			label: 'Save',
			api: UmbSaveWorkspaceAction,
		},
		conditions: {
			workspaces: ['Umb.Workspace.Language'],
		},
	},
];

export const manifests = [workspace, ...workspaceViews, ...workspaceActions];
