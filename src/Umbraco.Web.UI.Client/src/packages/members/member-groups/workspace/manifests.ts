import { UmbSaveWorkspaceAction } from '@umbraco-cms/backoffice/workspace';
import type {
	ManifestWorkspace,
	ManifestWorkspaceAction,
	ManifestWorkspaceEditorView,
} from '@umbraco-cms/backoffice/extension-registry';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: 'Umb.Workspace.MemberGroup',
	name: 'Member Group Workspace',
	loader: () => import('./member-group-workspace.element.js'),
	meta: {
		entityType: 'member-group',
	},
};

const workspaceViews: Array<ManifestWorkspaceEditorView> = [
	{
		type: 'workspaceEditorView',
		alias: 'Umb.WorkspaceView.MemberGroup.Info',
		name: 'Member Group Workspace Info View',
		loader: () => import('./views/info/workspace-view-member-group-info.element.js'),
		weight: 90,
		meta: {
			label: 'Info',
			pathname: 'info',
			icon: 'info',
			workspaces: ['Umb.Workspace.MemberGroup'],
		},
	},
];

const workspaceActions: Array<ManifestWorkspaceAction> = [
	{
		type: 'workspaceAction',
		alias: 'Umb.WorkspaceAction.MemberGroup.Save',
		name: 'Save Member Group Workspace Action',
		meta: {
			label: 'Save',
			look: 'primary',
			color: 'positive',
			api: UmbSaveWorkspaceAction,
			workspaces: ['Umb.Workspace.MemberGroup'],
		},
	},
];

export const manifests = [workspace, ...workspaceViews, ...workspaceActions];
