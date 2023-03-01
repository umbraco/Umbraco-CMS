import { TEMPLATE_REPOSITORY_ALIAS } from '../repository/manifests';
import { UmbSaveWorkspaceAction } from '@umbraco-cms/workspace';
import type { ManifestWorkspace, ManifestWorkspaceAction, ManifestWorkspaceView } from '@umbraco-cms/models';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: 'Umb.Workspace.Template',
	name: 'Template Workspace',
	loader: () => import('./template-workspace.element'),
	meta: {
		entityType: 'template',
	},
};

const workspaceViews: Array<ManifestWorkspaceView> = [];

const workspaceActions: Array<ManifestWorkspaceAction> = [
	{
		type: 'workspaceAction',
		alias: 'Umb.WorkspaceAction.Template.Save',
		name: 'Save Template',
		weight: 70,
		meta: {
			look: 'primary',
			color: 'positive',
			workspaces: ['Umb.Workspace.Template'],
			label: 'Save',
			api: UmbSaveWorkspaceAction,
		},
	},
];

export const manifests = [workspace, ...workspaceViews, ...workspaceActions];
