import { DICTIONARY_REPOSITORY_ALIAS } from '../repository/manifests';
import { UmbSaveWorkspaceAction } from '@umbraco-cms/workspace';
import type { ManifestWorkspace, ManifestWorkspaceAction, ManifestWorkspaceView } from '@umbraco-cms/models';

const workspaceAlias = 'Umb.Workspace.Dictionary';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: workspaceAlias,
	name: 'Dictionary Workspace',
	loader: () => import('./dictionary-workspace.element'),
	meta: {
		entityType: 'dictionary-item',
	},
};

const workspaceViews: Array<ManifestWorkspaceView> = [
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.Dictionary.Edit',
		name: 'Dictionary Workspace Edit View',
		loader: () => import('./views/edit/workspace-view-dictionary-edit.element'),
		weight: 100,
		meta: {
			workspaces: [workspaceAlias],
			label: 'Edit',
			pathname: 'edit',
			icon: 'edit',
		},
	},
];

const workspaceActions: Array<ManifestWorkspaceAction> = [
	{
		type: 'workspaceAction',
		alias: 'Umb.WorkspaceAction.Dictionary.Save',
		name: 'Save Dictionary Workspace Action',
		weight: 90,
		meta: {
			workspaces: ['Umb.Workspace.Dictionary'],
			label: 'Save',
			look: 'primary',
			color: 'positive',
			api: UmbSaveWorkspaceAction,
		},
	},
];

export const manifests = [workspace, ...workspaceViews, ...workspaceActions];
