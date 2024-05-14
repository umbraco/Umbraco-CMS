import { UmbSubmitWorkspaceAction } from '@umbraco-cms/backoffice/workspace';
import type {
	ManifestWorkspaces,
	ManifestWorkspaceActions,
	ManifestTypes,
} from '@umbraco-cms/backoffice/extension-registry';

export const UMB_PARTIAL_VIEW_WORKSPACE_ALIAS = 'Umb.Workspace.PartialView';

const workspace: ManifestWorkspaces = {
	type: 'workspace',
	kind: 'routable',
	alias: UMB_PARTIAL_VIEW_WORKSPACE_ALIAS,
	name: 'Partial View Workspace',
	api: () => import('./partial-view-workspace.context.js'),
	meta: {
		entityType: 'partial-view',
	},
};

const workspaceActions: Array<ManifestWorkspaceActions> = [
	{
		type: 'workspaceAction',
		kind: 'default',
		alias: 'Umb.WorkspaceAction.PartialView.Save',
		name: 'Save Partial View',
		api: UmbSubmitWorkspaceAction,
		meta: {
			label: '#buttons_save',
			look: 'primary',
			color: 'positive',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: workspace.alias,
			},
		],
	},
];

export const manifests: Array<ManifestTypes> = [workspace, ...workspaceActions];
