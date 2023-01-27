import type { ManifestWorkspace, ManifestWorkspaceAction, ManifestWorkspaceView } from '@umbraco-cms/models';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: 'Umb.Workspace.MemberGroup',
	name: 'Member Group Workspace',
	loader: () => import('./member-group-workspace.element'),
	meta: {
		entityType: 'member-group',
	},
};


const workspaceViews: Array<ManifestWorkspaceView> = [
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.MemberGroup.Info',
		name: 'Member Group Workspace Info View',
		loader: () => import('./views/info/workspace-view-member-group-info.element'),
		weight: 90,
		meta: {
			workspaces: ['Umb.Workspace.MemberGroup'],
			label: 'Info',
			pathname: 'info',
			icon: 'info',
		},
	},
];

const workspaceActions: Array<ManifestWorkspaceAction> = [
	{
		type: 'workspaceAction',
		alias: 'Umb.WorkspaceAction.MemberGroup.Save',
		name: 'Save Member Group Workspace Action',
		loader: () => import('src/backoffice/shared/components/workspace/actions/save/workspace-action-node-save.element'),
		meta: {
			workspaces: ['Umb.Workspace.MemberGroup'],
			look: 'primary',
			color: 'positive',
		},
	},
];

export const manifests = [workspace, ...workspaceViews, ...workspaceActions];
