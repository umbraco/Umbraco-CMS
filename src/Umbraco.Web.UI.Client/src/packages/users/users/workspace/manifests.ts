import { UmbSaveWorkspaceAction } from 'src/packages/core/components/workspace/workspace-action/common';
import type {
	ManifestWorkspace,
	ManifestWorkspaceAction,
	ManifestWorkspaceEditorView,
} from '@umbraco-cms/backoffice/extension-registry';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: 'Umb.Workspace.User',
	name: 'User Workspace',
	loader: () => import('./user-workspace.element'),
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
		},
		conditions: {
			workspaces: ['Umb.Workspace.User'],
		},
	},
];

export const manifests = [workspace, ...workspaceViews, ...workspaceActions];
