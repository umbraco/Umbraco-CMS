import { UmbSaveWorkspaceAction } from '@umbraco-cms/backoffice/workspace';
import type {
	ManifestWorkspace,
	ManifestWorkspaceAction,
	ManifestWorkspaceEditorView,
} from '@umbraco-cms/backoffice/extension-registry';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: 'Umb.Workspace.User',
	name: 'User Workspace',
	loader: () => import('./user-workspace.element.js'),
	meta: {
		entityType: 'user',
	},
};

const workspaceViews: Array<ManifestWorkspaceEditorView> = [];
const workspaceActions: Array<ManifestWorkspaceAction> = [
	{
		type: 'workspaceAction',
		alias: 'Umb.WorkspaceAction.User.Save',
		name: 'Save User Workspace Action',
		meta: {
			label: 'Save',
			look: 'primary',
			color: 'positive',
			api: UmbSaveWorkspaceAction,
			workspaces: ['Umb.Workspace.User'],
		},
	},
];

export const manifests = [workspace, ...workspaceViews, ...workspaceActions];
