import { UMB_USER_ENTITY_TYPE } from '../entity.js';
import { UmbSaveWorkspaceAction } from '@umbraco-cms/backoffice/workspace';
import type {
	ManifestWorkspaces,
	ManifestWorkspaceActions,
	ManifestWorkspaceView,
} from '@umbraco-cms/backoffice/extension-registry';

export const UMB_USER_WORKSPACE_ALIAS = 'Umb.Workspace.User';

const workspace: ManifestWorkspaces = {
	type: 'workspace',
	kind: 'routable',
	alias: UMB_USER_WORKSPACE_ALIAS,
	name: 'User Workspace',
	api: () => import('./user-workspace.context.js'),
	meta: {
		entityType: UMB_USER_ENTITY_TYPE,
	},
};

const workspaceViews: Array<ManifestWorkspaceView> = [];
const workspaceActions: Array<ManifestWorkspaceActions> = [
	{
		type: 'workspaceAction',
		kind: 'default',
		alias: 'Umb.WorkspaceAction.User.Save',
		name: 'Save User Workspace Action',
		api: UmbSaveWorkspaceAction,
		meta: {
			label: 'Save',
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

export const manifests = [workspace, ...workspaceViews, ...workspaceActions];
