import { UmbSaveWorkspaceAction } from '../../../../shared/workspace-actions/save.action';
import type { ManifestWorkspace, ManifestWorkspaceAction, ManifestWorkspaceView } from '@umbraco-cms/models';
import { LANGUAGE_REPOSITORY_ALIAS } from '../../repository/manifests';

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
			workspaces: ['Umb.Workspace.Language'],
			label: 'Edit',
			pathname: 'edit',
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
			repositoryAlias: LANGUAGE_REPOSITORY_ALIAS,
			api: UmbSaveWorkspaceAction,
		},
	},
];

export const manifests = [workspace, ...workspaceViews, ...workspaceActions];
