import { UmbSaveWorkspaceAction } from '@umbraco-cms/backoffice/workspace';
import type { ManifestWorkspace, ManifestWorkspaceAction } from '@umbraco-cms/backoffice/extensions-registry';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: 'Umb.Workspace.PartialView',
	name: 'Partial View Workspace',
	loader: () => import('./partial-views-workspace.element'),
	meta: {
		entityType: 'partial-view',
	},
};

const workspaceActions: Array<ManifestWorkspaceAction> = [
	{
		type: 'workspaceAction',
		alias: 'Umb.WorkspaceAction.PartialView.Save',
		name: 'Save Partial View',
		weight: 70,
		meta: {
			look: 'primary',
			color: 'positive',
			label: 'Save',
			api: UmbSaveWorkspaceAction,
		},
		conditions: {
			workspaces: ['Umb.Workspace.PartialView'],
		},
	},
];

export const manifests = [workspace, ...workspaceActions];
