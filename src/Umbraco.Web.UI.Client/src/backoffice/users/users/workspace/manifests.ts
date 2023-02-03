import type { ManifestWorkspace, ManifestWorkspaceAction, ManifestWorkspaceView } from '@umbraco-cms/models';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: 'Umb.Workspace.User',
	name: 'User Workspace',
	loader: () => import('./user-workspace.element'),
	meta: {
		entityType: 'user',
	},
};

const workspaceViews: Array<ManifestWorkspaceView> = [];
const workspaceActions: Array<ManifestWorkspaceAction> = [
	{
		type: 'workspaceAction',
		alias: 'Umb.WorkspaceAction.User.Save',
		name: 'Save User Workspace Action',
		loader: () =>
			import('src/backoffice/shared/components/workspace/workspace-action/save/workspace-action-node-save.element'),
		meta: {
			workspaces: ['Umb.Workspace.User'],
			look: 'primary',
			color: 'positive',
		},
	},
];

export const manifests = [workspace, ...workspaceViews, ...workspaceActions];
