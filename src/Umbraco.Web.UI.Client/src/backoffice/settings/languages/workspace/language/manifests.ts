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

const workspaceActions: Array<ManifestWorkspaceAction> = [
	{
		type: 'workspaceAction',
		alias: 'Umb.WorkspaceAction.Language.Save',
		name: 'Save Language Workspace Action',
		loader: () => import('src/backoffice/shared/components/workspace/actions/save/workspace-action-node-save.element'),
		meta: {
			workspaces: ['Umb.Workspace.Language'],
			look: 'primary',
			color: 'positive',
		},
	},
];

const workspaceViews: Array<ManifestWorkspaceView> = [
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.Language.Edit',
		name: 'Language Workspace Edit View',
		loader: () => import('./views/edit/workspace-view-language-edit.element'),
		weight: 90,
		meta: {
			workspaces: ['Umb.Workspace.Language'],
			label: 'Edit',
			pathname: 'edit',
			icon: 'edit',
		},
	},
];

export const manifests = [workspace, ...workspaceViews, ...workspaceActions];
