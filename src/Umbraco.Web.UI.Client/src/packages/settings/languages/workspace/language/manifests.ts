import { UmbSaveWorkspaceAction } from '@umbraco-cms/backoffice/workspace';
import type {
	ManifestWorkspace,
	ManifestWorkspaceAction,
	ManifestWorkspaceEditorView,
} from '@umbraco-cms/backoffice/extension-registry';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: 'Umb.Workspace.Language',
	name: 'Language Workspace',
	loader: () => import('./language-workspace.element'),
	meta: {
		entityType: 'language',
	},
};

const workspaceViews: Array<ManifestWorkspaceEditorView> = [
	{
		type: 'workspaceEditorView',
		alias: 'Umb.WorkspaceView.Language.Details',
		name: 'Language Workspace Details View',
		loader: () => import('./views/details/language-details-workspace-view.element'),
		weight: 90,
		meta: {
			label: 'Details',
			pathname: 'details',
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
